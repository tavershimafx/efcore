// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ClrPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected ClrPropertySetterFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ClrPropertySetterFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IClrPropertySetter Create(IPropertyBase property)
        => property as IClrPropertySetter ?? CreateBase(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IClrPropertySetter CreateGeneric<TEntity, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase)
    {
        CreateExpression<TEntity, TValue>(memberInfo, propertyBase, out var setter);
        return new ClrPropertySetter<TEntity, TValue>(setter.Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberInfo GetMemberInfo(IPropertyBase propertyBase)
        => propertyBase.GetMemberInfo(forMaterialization: false, forSet: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        IPropertyBase propertyBase,
        out Expression setterExpression)
    {
        var boundMethod = GenericCreateExpression.MakeGenericMethod(
            propertyBase.DeclaringType.ClrType,
            propertyBase.ClrType);

        try
        {
            var parameters = new object?[] { GetMemberInfo(propertyBase), propertyBase, null };
            boundMethod.Invoke(this, parameters);
            setterExpression = (Expression)parameters[2]!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpression
        = typeof(ClrPropertySetterFactory).GetMethod(nameof(CreateExpression), BindingFlags.Instance | BindingFlags.NonPublic)!;


    private void CreateExpression<TEntity, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        out Expression<Action<TEntity, TValue>> setter) where TEntity : class
    {
        var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
        var valueParameter = Expression.Parameter(typeof(TValue), "value");

        var memberType = memberInfo.GetMemberType();
        Expression convertedParameter = memberType == typeof(TValue)
            ? valueParameter
            : Expression.Convert(valueParameter, memberType);
        Expression writeExpression;
        if (memberInfo.DeclaringType!.IsAssignableFrom(typeof(TEntity)))
        {
            writeExpression = CreateMemberAssignment(entityParameter);
        }
        else
        {
            // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
            var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

            writeExpression = Expression.Block(
                new[] { converted },
                new List<Expression>
                {
                    Expression.Assign(
                        converted,
                        Expression.TypeAs(entityParameter, memberInfo.DeclaringType)),
                    Expression.IfThen(
                        Expression.ReferenceNotEqual(converted, Expression.Constant(null)),
                        CreateMemberAssignment(converted))
                });
        }

        setter = Expression.Lambda<Action<TEntity, TValue>>(
            writeExpression,
            entityParameter,
            valueParameter);

        Expression CreateMemberAssignment(Expression parameter)
            => propertyBase?.IsIndexerProperty() == true
                ? Expression.Assign(
                    Expression.MakeIndex(
                        entityParameter, (PropertyInfo)memberInfo, new List<Expression> { Expression.Constant(propertyBase.Name) }),
                    convertedParameter)
                : Expression.MakeMemberAccess(parameter, memberInfo).Assign(convertedParameter);
    }
}
