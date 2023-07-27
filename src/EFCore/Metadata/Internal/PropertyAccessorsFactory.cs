// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PropertyAccessorsFactory
{
    private PropertyAccessorsFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly PropertyAccessorsFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessors Create(IPropertyBase propertyBase)
        => (PropertyAccessors)GenericCreate
            .MakeGenericMethod(propertyBase.ClrType)
            .Invoke(null, new object[] { propertyBase })!;

    private static readonly MethodInfo GenericCreate
        = typeof(PropertyAccessorsFactory).GetMethod(nameof(CreateGeneric), BindingFlags.Static | BindingFlags.NonPublic)!;

    [UsedImplicitly]
    private static PropertyAccessors CreateGeneric<TProperty>(IPropertyBase propertyBase)
    {
        var property = propertyBase as IProperty;

        return new PropertyAccessors(
            CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: true).Compile(),
            CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: false).Compile(),
            property == null ? null : CreateOriginalValueGetter<TProperty>(property).Compile(),
            CreateRelationshipSnapshotGetter<TProperty>(propertyBase).Compile(),
            property == null ? null : CreateValueBufferGetter(property).Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        IPropertyBase propertyBase,
        out Expression currentValueGetter,
        out Expression preStoreGeneratedCurrentValueGetter,
        out Expression? originalValueGetter,
        out Expression relationshipSnapshotGetter,
        out Expression? valueBufferGetter)
    {
        var boundMethod = GenericCreateExpressions.MakeGenericMethod(propertyBase.ClrType);

        try
        {
            var parameters = new object?[] { propertyBase, null, null, null, null, null };
            boundMethod.Invoke(this, parameters);
            currentValueGetter = (Expression)parameters[1]!;
            preStoreGeneratedCurrentValueGetter = (Expression)parameters[2]!;
            originalValueGetter = (Expression?)parameters[3];
            relationshipSnapshotGetter = (Expression)parameters[4]!;
            valueBufferGetter = (Expression?)parameters[5];
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpressions
        = typeof(PropertyAccessorsFactory).GetMethod(nameof(CreateExpressions), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private void CreateExpressions<TProperty>(
        IPropertyBase propertyBase,
        out Expression<Func<IInternalEntry, TProperty>> currentValueGetter,
        out Expression<Func<IInternalEntry, TProperty>> preStoreGeneratedCurrentValueGetter,
        out Expression<Func<IInternalEntry, TProperty>>? originalValueGetter,
        out Expression<Func<InternalEntityEntry, TProperty>> relationshipSnapshotGetter,
        out Expression<Func<ValueBuffer, object>>? valueBufferGetter)
    {
        var property = propertyBase as IProperty;
        currentValueGetter = CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: true);
        preStoreGeneratedCurrentValueGetter = CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: false);
        originalValueGetter = property == null ? null : CreateOriginalValueGetter<TProperty>(property);
        relationshipSnapshotGetter = CreateRelationshipSnapshotGetter<TProperty>(propertyBase);
        valueBufferGetter = property == null ? null : CreateValueBufferGetter(property);
    }

    private static Expression<Func<IInternalEntry, TProperty>> CreateCurrentValueGetter<TProperty>(
        IPropertyBase propertyBase,
        bool useStoreGeneratedValues)
    {
        var entityClrType = propertyBase.DeclaringType.ClrType;
        var entryParameter = Expression.Parameter(typeof(IInternalEntry), "entry");
        var propertyIndex = propertyBase.GetIndex();
        var shadowIndex = propertyBase.GetShadowIndex();
        var storeGeneratedIndex = propertyBase.GetStoreGeneratedIndex();
        Expression currentValueExpression;
        Expression hasSentinelValueExpression;

        if (shadowIndex >= 0)
        {
            currentValueExpression = Expression.Call(
                entryParameter,
                InternalEntityEntry.MakeReadShadowValueMethod(typeof(TProperty)),
                Expression.Constant(shadowIndex));

            hasSentinelValueExpression = currentValueExpression.MakeHasSentinelValue(propertyBase);
        }
        else
        {
            var convertedExpression = Expression.Convert(
                Expression.Property(entryParameter, nameof(IInternalEntry.Object)),
                entityClrType);

            var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);

            currentValueExpression = CreateMemberAccess(propertyBase, convertedExpression, memberInfo);
            hasSentinelValueExpression = currentValueExpression.MakeHasSentinelValue(propertyBase);

            if (currentValueExpression.Type != typeof(TProperty))
            {
                if (currentValueExpression.Type.IsNullableType()
                    && !typeof(TProperty).IsNullableType())
                {
                    var nullableValue = Expression.Variable(currentValueExpression.Type, "nullableValue");

                    currentValueExpression = Expression.Block(
                        new[] { nullableValue },
                        new List<Expression>
                        {
                            Expression.Assign(
                                nullableValue,
                                currentValueExpression),
                            currentValueExpression.Type.IsValueType
                                ? Expression.Condition(
                                    Expression.MakeMemberAccess(
                                        nullableValue,
                                        nullableValue.Type.GetProperty("HasValue")!),
                                    Expression.Convert(nullableValue, typeof(TProperty)),
                                    Expression.Default(typeof(TProperty)))
                                : Expression.Condition(
                                    Expression.ReferenceEqual(nullableValue, Expression.Constant(null)),
                                    Expression.Default(typeof(TProperty)),
                                    Expression.Convert(nullableValue, typeof(TProperty)))
                        });
                }
                else
                {
                    currentValueExpression = Expression.Convert(currentValueExpression, typeof(TProperty));
                }
            }
        }

        if (useStoreGeneratedValues && storeGeneratedIndex >= 0)
        {
            currentValueExpression = Expression.Condition(
                Expression.Call(
                    entryParameter,
                    InternalEntityEntry.FlaggedAsStoreGeneratedMethod,
                    Expression.Constant(propertyIndex)),
                Expression.Call(
                    entryParameter,
                    InternalEntityEntry.MakeReadStoreGeneratedValueMethod(typeof(TProperty)),
                    Expression.Constant(storeGeneratedIndex)),
                Expression.Condition(
                    Expression.AndAlso(
                        Expression.Call(
                            entryParameter,
                            InternalEntityEntry.FlaggedAsTemporaryMethod,
                            Expression.Constant(propertyIndex)),
                        hasSentinelValueExpression),
                    Expression.Call(
                        entryParameter,
                        InternalEntityEntry.MakeReadTemporaryValueMethod(typeof(TProperty)),
                        Expression.Constant(storeGeneratedIndex)),
                    currentValueExpression));
        }

        return Expression.Lambda<Func<IInternalEntry, TProperty>>(
                currentValueExpression,
                entryParameter);
    }

    private static Expression<Func<IInternalEntry, TProperty>> CreateOriginalValueGetter<TProperty>(IProperty property)
    {
        var entryParameter = Expression.Parameter(typeof(IInternalEntry), "entry");
        var originalValuesIndex = property.GetOriginalValueIndex();

        return Expression.Lambda<Func<IInternalEntry, TProperty>>(
                originalValuesIndex >= 0
                    ? Expression.Call(
                        entryParameter,
                        InternalEntityEntry.MakeReadOriginalValueMethod(typeof(TProperty)),
                        Expression.Constant(property),
                        Expression.Constant(originalValuesIndex))
                    : Expression.Block(
                        Expression.Throw(
                            Expression.Constant(
                                new InvalidOperationException(
                                    CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringType.DisplayName())))),
                        Expression.Constant(default(TProperty), typeof(TProperty))),
                entryParameter);
    }

    private static Expression<Func<InternalEntityEntry, TProperty>> CreateRelationshipSnapshotGetter<TProperty>(IPropertyBase propertyBase)
    {
        var entryParameter = Expression.Parameter(typeof(InternalEntityEntry), "entry");
        var relationshipIndex = (propertyBase as IProperty)?.GetRelationshipIndex() ?? -1;

        return Expression.Lambda<Func<InternalEntityEntry, TProperty>>(
                relationshipIndex >= 0
                    ? Expression.Call(
                        entryParameter,
                        InternalEntityEntry.MakeReadRelationshipSnapshotValueMethod(typeof(TProperty)),
                        Expression.Constant(propertyBase),
                        Expression.Constant(relationshipIndex))
                    : Expression.Call(
                        entryParameter,
                        InternalEntityEntry.MakeGetCurrentValueMethod(typeof(TProperty)),
                        Expression.Constant(propertyBase)),
                entryParameter);
    }

    private static Expression<Func<ValueBuffer, object>> CreateValueBufferGetter(IProperty property)
    {
        var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer), "valueBuffer");

        return Expression.Lambda<Func<ValueBuffer, object>>(
            Expression.MakeIndex(
                valueBufferParameter,
                ValueBuffer.Indexer,
                new[] { Expression.Constant(property.GetIndex()) }),
            valueBufferParameter);
    }


    private static readonly MethodInfo ContainsKeyMethod =
        typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey), new[] { typeof(string) })!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression CreateMemberAccess(
        IPropertyBase? property,
        Expression instanceExpression,
        MemberInfo memberInfo)
    {
        if (property?.IsIndexerProperty() == true)
        {
            Expression expression = Expression.MakeIndex(
                instanceExpression, (PropertyInfo)memberInfo, new List<Expression> { Expression.Constant(property.Name) });

            if (property.DeclaringType.IsPropertyBag)
            {
                expression = Expression.Condition(
                    Expression.Call(
                        instanceExpression, ContainsKeyMethod, new List<Expression> { Expression.Constant(property.Name) }),
                    expression,
                    expression.Type.GetDefaultValueConstant());
            }

            return expression;
        }

        return Expression.MakeMemberAccess(instanceExpression, memberInfo);
    }
}
