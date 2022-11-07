// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
internal class CollectionIndexerToElementAtConvertingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // Convert list[x] to list.ElementAt(x)
        if (methodCallExpression.Method is { Name: "get_Item", IsStatic: false, DeclaringType: { IsGenericType: true } declaringType }
            && declaringType.GetGenericTypeDefinition() == typeof(List<>)
            && ShouldConvert(methodCallExpression.Type))
        {
            var source = Visit(methodCallExpression.Object!);
            var index = Visit(methodCallExpression.Arguments[0]);
            var sourceTypeArgument = source.Type.GetSequenceType();

            return Expression.Call(
                QueryableMethods.ElementAt.MakeGenericMethod(sourceTypeArgument),
                    Expression.Call(
                        QueryableMethods.AsQueryable.MakeGenericMethod(sourceTypeArgument),
                        source),
                    index);
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        // Convert array[x] to list.ElementAt(x)
        if (binaryExpression.NodeType == ExpressionType.ArrayIndex
            && ShouldConvert(binaryExpression.Type))
        {
            var source = Visit(binaryExpression.Left);
            var index = Visit(binaryExpression.Right);
            var sourceTypeArgument = source.Type.GetSequenceType();

            return Expression.Call(
                QueryableMethods.ElementAt.MakeGenericMethod(sourceTypeArgument),
                    Expression.Call(
                        QueryableMethods.AsQueryable.MakeGenericMethod(sourceTypeArgument),
                        source),
                    index);
        }

        return base.VisitBinary(binaryExpression);
    }

    private static bool ShouldConvert(Type type)
        => !type.IsPrimitive && !type.IsEnum && type != typeof(object) && type != typeof(string);
}
