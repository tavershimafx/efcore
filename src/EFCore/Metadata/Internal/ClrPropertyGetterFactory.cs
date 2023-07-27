// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ClrPropertyGetterFactory : ClrAccessorFactory<IClrPropertyGetter>
{
    private ClrPropertyGetterFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ClrPropertyGetterFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IClrPropertyGetter Create(IPropertyBase property)
        => property as IClrPropertyGetter ?? CreateBase(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IClrPropertyGetter CreateGeneric<TEntity, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase)
    {
        CreateExpressions<TEntity, TValue>(memberInfo, propertyBase, out var getterExpression, out var hasSentinelExpression);
        return new ClrPropertyGetter<TEntity, TValue>(getterExpression.Compile(), hasSentinelExpression.Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberInfo GetMemberInfo(IPropertyBase propertyBase)
        => propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        IPropertyBase propertyBase,
        out Expression getterExpression,
        out Expression hasSentinelExpression)
    {
        var boundMethod = GenericCreateExpressions.MakeGenericMethod(
            propertyBase.DeclaringType.ClrType,
            propertyBase.ClrType);

        try
        {
            var parameters = new object?[] { GetMemberInfo(propertyBase), propertyBase, null, null };
            boundMethod.Invoke(this, parameters);
            getterExpression = (Expression)parameters[2]!;
            hasSentinelExpression = (Expression)parameters[3]!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpressions
        = typeof(ClrPropertyGetterFactory).GetMethod(nameof(CreateExpressions), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private void CreateExpressions<TEntity, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase,
        out Expression<Func<TEntity, TValue>> getterExpression,
        out Expression<Func<TEntity, bool>> hasSentinelExpression)
    {
        var entityParameter = Expression.Parameter(typeof(TEntity), "entity");

        Expression readExpression;
        if (memberInfo.DeclaringType!.IsAssignableFrom(typeof(TEntity)))
        {
            readExpression = PropertyAccessorsFactory.CreateMemberAccess(propertyBase, entityParameter, memberInfo);
        }
        else
        {
            // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
            var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

            readExpression = Expression.Block(
                new[] { converted },
                new List<Expression>
                {
                    Expression.Assign(
                        converted,
                        Expression.TypeAs(entityParameter, memberInfo.DeclaringType)),
                    Expression.Condition(
                        Expression.ReferenceEqual(converted, Expression.Constant(null)),
                        Expression.Default(memberInfo.GetMemberType()),
                        PropertyAccessorsFactory.CreateMemberAccess(propertyBase, converted, memberInfo))
                });
        }

        var hasSentinelValueExpression = readExpression.MakeHasSentinelValue(propertyBase);

        if (readExpression.Type != typeof(TValue))
        {
            readExpression = Expression.Condition(
                hasSentinelValueExpression,
                Expression.Constant(default(TValue), typeof(TValue)),
                Expression.Convert(readExpression, typeof(TValue)));
        }

        getterExpression = Expression.Lambda<Func<TEntity, TValue>>(readExpression, entityParameter);
        hasSentinelExpression = Expression.Lambda<Func<TEntity, bool>>(hasSentinelValueExpression, entityParameter);
    }
}
