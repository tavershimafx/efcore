// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OperatorsQuerySqlServerTest : OperatorsQueryTestBase
{
    private static readonly MethodInfo AtTimeZoneDateTimeOffsetMethodInfo = typeof(SqlServerDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SqlServerDbFunctionsExtensions.AtTimeZone),
            new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(string) })!;


    public OperatorsQuerySqlServerTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        //Binaries.AddRange(new List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)>
        //{
        //    ((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.LessThan),
        //    ((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.LessThanOrEqual),
        //    ((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.GreaterThan),
        //    ((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.GreaterThanOrEqual),
        //    ((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.Equal),
        //    ((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.NotEqual),
        //});

        //Unaries.Add((typeof(DateTimeOffset), typeof(DateTimeOffset), x => Expression.Call(
        //    null,
        //    AtTimeZoneDateTimeOffsetMethodInfo,
        //    Expression.Constant(EF.Functions),
        //    x,
        //    Expression.Constant("UTC"))));

        ExpectedQueryRewriter = new SqlServerExpectedQueryRewritingVisitor();
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected class SqlServerExpectedQueryRewritingVisitor : ExpectedQueryRewritingVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method == AtTimeZoneDateTimeOffsetMethodInfo
                && methodCallExpression.Arguments[2] is ConstantExpression { Value: "UTC" })
            {
                var inner = Visit(methodCallExpression.Arguments[1]);

                return Expression.Convert(
                    Expression.Property(inner, nameof(DateTimeOffset.UtcDateTime)),
                    typeof(DateTimeOffset));
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
