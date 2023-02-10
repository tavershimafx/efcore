// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OperatorsQueryTestBase : NonSharedModelTestBase
{
    protected readonly List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> Binaries;
    protected readonly List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> Unaries;
    protected readonly List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> FakeUnaries;
    protected readonly Dictionary<Type, Type> PropertyTypeToEntityMap;

    protected OperatorsData ExpectedData { get; init; }
    protected ExpectedQueryRewritingVisitor ExpectedQueryRewriter { get; init; }

    private static readonly MethodInfo _likeMethodInfo
        = typeof(DbFunctionsExtensions).GetRuntimeMethod(
            nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

    private static readonly MethodInfo _stringConcatMethodInfo
        = typeof(string).GetRuntimeMethod(
            nameof(string.Concat), new[] { typeof(string), typeof(string) });


    protected OperatorsQueryTestBase(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        Binaries = new()
        {
            ((typeof(string), typeof(string)), typeof(bool), Expression.Equal),
            ((typeof(string), typeof(string)), typeof(bool), Expression.NotEqual),
            ((typeof(string), typeof(string)), typeof(string), (x, y) => Expression.Add(x, y, _stringConcatMethodInfo)),
            ((typeof(string), typeof(string)), typeof(bool), (x, y) => Expression.Call(
                null,
                _likeMethodInfo,
                Expression.Constant(EF.Functions),
                x,
                y)),

            ((typeof(int), typeof(int)), typeof(int), Expression.Multiply),
            ((typeof(int), typeof(int)), typeof(int), Expression.Divide),
            ((typeof(int), typeof(int)), typeof(int), Expression.Modulo),
            ((typeof(int), typeof(int)), typeof(int), Expression.Add),
            ((typeof(int), typeof(int)), typeof(int), Expression.Subtract),
            ((typeof(int), typeof(int)), typeof(bool), Expression.Equal),
            ((typeof(int), typeof(int)), typeof(bool), Expression.NotEqual),
            ((typeof(int), typeof(int)), typeof(bool), Expression.LessThan),
            ((typeof(int), typeof(int)), typeof(bool), Expression.LessThanOrEqual),
            ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThan),
            ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThanOrEqual),

            //((typeof(int), typeof(int)), typeof(int), Expression.And),
            //((typeof(int), typeof(int)), typeof(int), Expression.Or),

            //((typeof(int), typeof(int)), typeof(int), Expression.LeftShift),
            //((typeof(int), typeof(int)), typeof(int), Expression.RightShift),

            ((typeof(long), typeof(long)), typeof(long), Expression.Multiply),
            ((typeof(long), typeof(long)), typeof(long), Expression.Divide),
            ((typeof(long), typeof(long)), typeof(long), Expression.Modulo),
            ((typeof(long), typeof(long)), typeof(long), Expression.Add),
            ((typeof(long), typeof(long)), typeof(long), Expression.Subtract),
            ((typeof(long), typeof(long)), typeof(bool), Expression.Equal),
            ((typeof(long), typeof(long)), typeof(bool), Expression.NotEqual),
            ((typeof(long), typeof(long)), typeof(bool), Expression.LessThan),
            ((typeof(long), typeof(long)), typeof(bool), Expression.LessThanOrEqual),
            ((typeof(long), typeof(long)), typeof(bool), Expression.GreaterThan),
            ((typeof(long), typeof(long)), typeof(bool), Expression.GreaterThanOrEqual),

            //((typeof(long), typeof(long)), typeof(long), Expression.And),
            //((typeof(long), typeof(long)), typeof(long), Expression.Or),

            ((typeof(bool), typeof(bool)), typeof(bool), Expression.And),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Or),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Equal),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.NotEqual),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.AndAlso),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.OrElse),
        };

        Unaries = new()
        {
            (typeof(string), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(string)))),
            (typeof(string), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(string)))),
            (typeof(string), typeof(bool), x => Expression.Call(
                null,
                _likeMethodInfo,
                Expression.Constant(EF.Functions),
                x,
                Expression.Constant("A%"))),
            (typeof(string), typeof(bool), x => Expression.Call(
                null,
                _likeMethodInfo,
                Expression.Constant(EF.Functions),
                x,
                Expression.Constant("%B"))),

            (typeof(int), typeof(int), Expression.Not),
            (typeof(int), typeof(int), Expression.Negate),
            (typeof(int), typeof(long), x => Expression.Convert(x, typeof(long))),

            (typeof(int?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(int?)))),
            (typeof(int?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(int?)))),

            (typeof(long), typeof(long), Expression.Not),
            (typeof(long), typeof(long), Expression.Negate),
            (typeof(long), typeof(int), x => Expression.Convert(x, typeof(int))),

            (typeof(bool), typeof(bool), Expression.Not),

            (typeof(bool?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(bool?)))),
            (typeof(bool?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(bool?)))),
        };

        PropertyTypeToEntityMap = new()
        {
            { typeof(string), typeof(OperatorEntityString) },
            { typeof(int), typeof(OperatorEntityInt) },
            { typeof(int?), typeof(OperatorEntityNullableInt) },
            { typeof(long), typeof(OperatorEntityLong) },
            { typeof(bool), typeof(OperatorEntityBool) },
            { typeof(bool?), typeof(OperatorEntityNullableBool) },
            { typeof(DateTimeOffset), typeof(OperatorEntityDateTimeOffset) },
        };

        ExpectedData = OperatorsData.Instance;
        ExpectedQueryRewriter = new ExpectedQueryRewritingVisitor();
    }

    protected override string StoreName
        => "OperatorsTest";

    protected virtual void Seed(OperatorsContext ctx)
    {
        ctx.Set<OperatorEntityString>().AddRange(ExpectedData.OperatorEntitiesString);
        ctx.Set<OperatorEntityInt>().AddRange(ExpectedData.OperatorEntitiesInt);
        ctx.Set<OperatorEntityNullableInt>().AddRange(ExpectedData.OperatorEntitiesNullableInt);
        ctx.Set<OperatorEntityLong>().AddRange(ExpectedData.OperatorEntitiesLong);
        ctx.Set<OperatorEntityBool>().AddRange(ExpectedData.OperatorEntitiesBool);
        ctx.Set<OperatorEntityNullableBool>().AddRange(ExpectedData.OperatorEntitiesNullableBool);
        ctx.Set<OperatorEntityDateTimeOffset>().AddRange(ExpectedData.OperatorEntitiesDateTimeOffset);

        ctx.SaveChanges();
    }

    [ConditionalFact]
    public virtual async Task Regression_test1()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var expected = (from o1 in ExpectedData.OperatorEntitiesString
                            from o2 in ExpectedData.OperatorEntitiesString
                            from o3 in ExpectedData.OperatorEntitiesBool
                            where ((o2.Value == "B" || o3.Value) & (o1.Value != null)) != false
                            select new { Value1 = o1.Value, Value2 = o2.Value, Value3 = o3.Value }).ToList();

            var actual = (from o1 in context.Set<OperatorEntityString>()
                          from o2 in context.Set<OperatorEntityString>()
                          from o3 in context.Set<OperatorEntityBool>()
                          where ((EF.Functions.Like(o2.Value, "B") || o3.Value) & (o1.Value != null)) != false
                          select new { Value1 = o1.Value, Value2 = o2.Value, Value3 = o3.Value }).ToList();
        }
    }

    [ConditionalFact]
    public virtual async Task Procedural_predicate_six_sources_random()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);

            while (true)
            //for (var index = 0; index < 100000; index++)
            {
                var seed = new Random().Next();


                //seed = 1726537739; //<- with and/ors on int/long



                //seed = 542103523; // without







                var random = new Random(seed);
                var maxDepth = 7;

                var possibleTypes = OperatorsData.Instance.ConstantExpressionsPerType.Keys.ToArray();

                var typesUsed = new bool[6];
                var types = new Type[6];
                for (var i = 0; i < types.Length; i++)
                {
                    types[i] = possibleTypes[random.Next(possibleTypes.Length)];
                    types[i + 1] = types[i];
                    i++;
                }

                // dummy input expression and whether is has already been used
                // (we want to prioritize ones that haven't been used yet, so that generated expressions are more interesting)
                var rootEntityExpressions = types.Select((x, i) => new RootEntityExpressionInfo(
                    Expression.Property(
                        Expression.Parameter(PropertyTypeToEntityMap[x], "e" + i),
                        "Value"))).ToArray();

                var distinctTypes = types.Distinct().ToList();
                var possibleLeafBinaries = Binaries.Where(x => distinctTypes.Contains(x.InputTypes.Item1) && distinctTypes.Contains(x.InputTypes.Item2)).ToList();
                var possibleLeafUnaries = Unaries.Where(x => distinctTypes.Contains(x.InputType)).ToList();

                // we assume one level of nesting is enough to get to all possible operations
                // this should be true, since all operations either result in bool or the same type as input
                // only exception being convert, which needs one step to get to all possible options: long -> int, or int -> long
                var distinctTypesWithNesting = distinctTypes
                    .Concat(possibleLeafBinaries.Select(x => x.ResultType))
                    .Concat(possibleLeafUnaries.Select(x => x.ResultType))
                    .Distinct()
                    .ToList();

                var possibleBinaries = Binaries.Where(x => distinctTypesWithNesting.Contains(x.InputTypes.Item1) && distinctTypesWithNesting.Contains(x.InputTypes.Item2)).ToList();
                var possibleUnaries = Unaries.Where(x => distinctTypesWithNesting.Contains(x.InputType)).ToList();

                var currentDepth = 0;
                var currentResultType = typeof(bool);

                // main loop
                var resultExpression = MainLoop(
                    random,
                    currentResultType,
                    currentDepth,
                    maxDepth,
                    types,
                    rootEntityExpressions,
                    possibleBinaries,
                    possibleUnaries);

                try
                {
                    TestPredicateQuery(
                        actualSetSource,
                        rootEntityExpressions.Where(x => x.Used).Select(x => x.Expression).ToArray(),
                        resultExpression);
                }
                catch (Exception ex)
                {
                    // TODO: hack instead make sure expected results also throw divide by 0
                    if (ex.InnerException != null && ex.InnerException.Message.ToLower().Contains("divide by zero")
                        || ex.InnerException != null && ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message.ToLower().Contains("divide by zero"))
                    {
                    }
                    else
                    {
                        throw new InvalidOperationException("Seed: " + seed, ex);
                    }
                }
            }
        }
    }

    private class RootEntityExpressionInfo
    {
        public RootEntityExpressionInfo(Expression expression)
        {
            Expression = expression;
            Used = false;
        }

        public Expression Expression { get; }

        public bool Used { get; set; }
    }

    private Expression MainLoop(
        Random random,
        Type currentResultType,
        int currentDepth,
        int maxDepth,
        Type[] types,
        RootEntityExpressionInfo[] rootPropertyExpressions,
        List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> possibleBinaries,
        List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> possibleUnaries)
    {
        // see if we want additional level of nesting, the deeper we go the lower the probability
        // we also force nesting if we end up with an expected node that we don't have the root entity for
        // this can happen when we use convert - e.g. we only have int sources, but we expect long
        var rollAddDepth = random.Next(maxDepth);
        if (rollAddDepth >= currentDepth)// || !rootPropertyExpressions.Any(x => x.Expression.Type == currentResultType))
        {
            //if (rollAddDepth < currentDepth)
            //{


            //    // if we get here try to get out as soon as possible

            //    var operation = possibleUnaries.Where(x => x.ResultType == currentResultType && rootPropertyExpressions.Any(xx => xx.Expression.Type == x.InputType)).FirstOrDefault();
            //    if (operation.InputType == null)
            //    {
            //        throw new InvalidOperationException("why are we here?");
            //    }

            //    //var operation = possibleUnaries.Where(x => x.ResultType == currentResultType && rootPropertyExpressions.Any(xx => xx.Expression.Type == x.InputType)).First();

            //    return AddUnaryOperation(
            //        random,
            //        currentDepth,
            //        maxDepth,
            //        operation,
            //        types,
            //        rootPropertyExpressions,
            //        possibleBinaries,
            //        possibleUnaries);
            //}


            var possibleBinariesForResultType = possibleBinaries.Where(x => x.ResultType == currentResultType).ToList();
            var possibleUnariesForResultType = possibleUnaries.Where(x => x.ResultType == currentResultType).ToList();

            // if we can't go any deeper (no matching operations) then simply return source 
            if (possibleBinariesForResultType.Count == 0 && possibleUnariesForResultType.Count == 0)
            {
                return AddRootPropertyAccess(random, currentResultType, rootPropertyExpressions);
            }

            var operationIndex = random.Next(possibleBinariesForResultType.Count + possibleUnariesForResultType.Count);
            if (operationIndex < possibleBinariesForResultType.Count)
            {
                var operation = possibleBinariesForResultType[operationIndex];
                return AddBinaryOperation(
                    random,
                    currentDepth,
                    maxDepth,
                    operation,
                    types,
                    rootPropertyExpressions,
                    possibleBinaries,
                    possibleUnaries);
            }
            else
            {
                var operation = possibleUnariesForResultType[operationIndex - possibleBinariesForResultType.Count];
                return AddUnaryOperation(
                    random,
                    currentDepth,
                    maxDepth,
                    operation,
                    types,
                    rootPropertyExpressions,
                    possibleBinaries,
                    possibleUnaries);
            }
        }
        else
        {
            return AddRootPropertyAccess(random, currentResultType, rootPropertyExpressions);
        }
    }

    private Expression AddRootPropertyAccess(
        Random random,
        Type currentResultType,
        RootEntityExpressionInfo[] rootEntityExpressions)
    {
        // just pick a source, prioritize sources that were not used yet
        var matchingExpressions = rootEntityExpressions.Where(x => x.Expression.Type == currentResultType).ToList();

        // if we want to break, but don't we don't have any roots that match the criteria just return a constant
        // to simplify the logic here
        if (matchingExpressions.Count == 0)
        {
            var constants = OperatorsData.Instance.ConstantExpressionsPerType[currentResultType];

            return constants[random.Next(constants.Count)];
        }

        var unusedExpressions = matchingExpressions.Where(x => !x.Used).ToList();
        if (unusedExpressions.Any())
        {
            var chosenExpresion = unusedExpressions[random.Next(unusedExpressions.Count)];
            chosenExpresion.Used = true;

            return chosenExpresion.Expression;
        }
        else
        {
            return matchingExpressions[random.Next(matchingExpressions.Count)].Expression;
        }
    }

    private Expression AddBinaryOperation(
        Random random,
        int currentDepth,
        int maxDepth,
        ((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator) operation,
        Type[] types,
        RootEntityExpressionInfo[] rootPropertyExpressions,
        List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> possibleBinaries,
        List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> possibleUnaries)
    {
        currentDepth++;
        var left = MainLoop(
            random,
            operation.InputTypes.Item1,
            currentDepth,
            maxDepth,
            types,
            rootPropertyExpressions,
            possibleBinaries,
            possibleUnaries);

        Expression right;
        var rollFakeBinary = random.Next(3);
        if (rollFakeBinary > 1)
        {
            var constants = OperatorsData.Instance.ConstantExpressionsPerType[operation.InputTypes.Item2];
            right = constants.Skip(random.Next(constants.Count)).First();
        }
        else
        {
            right = MainLoop(
                random,
                operation.InputTypes.Item2,
                currentDepth,
                maxDepth,
                types,
                rootPropertyExpressions,
                possibleBinaries,
                possibleUnaries);
        }

        return operation.OperatorCreator(left, right);
    }

    private Expression AddUnaryOperation(
        Random random,
        int currentDepth,
        int maxDepth,
        (Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator) operation,
        Type[] types,
        RootEntityExpressionInfo[] rootPropertyExpressions,
        List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> possibleBinaries,
        List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> possibleUnaries)
    {
        currentDepth++;
        var source = MainLoop(
            random,
            operation.InputType,
            currentDepth,
            maxDepth,
            types,
            rootPropertyExpressions,
            possibleBinaries,
            possibleUnaries);

        return operation.OperatorCreator(source);
    }

    protected class ExpectedQueryRewritingVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(
                nameof(string.StartsWith), new[] { typeof(string) })!;

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(
                nameof(string.EndsWith), new[] { typeof(string) })!;


        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method == _likeMethodInfo)
            {
                if (methodCallExpression.Arguments[2] is ConstantExpression { Value: "A%" })
                {
                    return Expression.Call(
                        methodCallExpression.Arguments[1],
                        _startsWithMethodInfo,
                        Expression.Constant("A"));
                }

                if (methodCallExpression.Arguments[2] is ConstantExpression { Value: "%B" })
                {
                    return Expression.Call(
                        methodCallExpression.Arguments[1],
                        _endsWithMethodInfo,
                        Expression.Constant("B"));
                }



                return Expression.Equal(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2]);

            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    #region projection

    private void TestProjectionQueryWithSixSources(
        ISetSource actualSetSource,
        Type type1,
        Type type2,
        Type type3,
        Type type4,
        Type type5,
        Type type6,
        Type resultType,
        Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> resultCreator)
    {
        var method = typeof(OperatorsQueryTestBase).GetMethod(
            nameof(TestProjectionQueryWithSixSourcesInternal),
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = method.MakeGenericMethod(
            PropertyTypeToEntityMap[type1],
            PropertyTypeToEntityMap[type2],
            PropertyTypeToEntityMap[type3],
            PropertyTypeToEntityMap[type4],
            PropertyTypeToEntityMap[type5],
            PropertyTypeToEntityMap[type6],
            resultType);

        genericMethod.Invoke(
            this,
            new object[]
            {
                actualSetSource,
                resultCreator
            });
    }

    private class ActualSetSource : ISetSource
    {
        private readonly DbContext _context;

        public ActualSetSource(DbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();
    }

    private void TestProjectionQueryWithSixSourcesInternal<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>(
        ISetSource actualSetSource,
        Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> resultCreator)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
        where TEntity6 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            from e5 in ss.Set<TEntity5>()
            from e6 in ss.Set<TEntity6>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id, e5.Id, e6.Id
            select new OperatorDto6<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>(e1, e2, e3, e4, e5, e6, default);

        var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<OperatorDto6<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<OperatorDto6<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Result, expectedResults[i].Result);
        }
    }

    private class ResultExpressionProjectionRewriter : ExpressionVisitor
    {
        private readonly Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> _resultCreatorSixArgs;

        public ResultExpressionProjectionRewriter(Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> resultCreatorSixArgs)
        {
            _resultCreatorSixArgs = resultCreatorSixArgs;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            if (newExpression.Constructor is ConstructorInfo ctorInfo
                && ctorInfo.DeclaringType is Type { IsGenericType: true } declaringType)
            {
                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto6<,,,,,,>))
                {
                    var argumentValue1 = Expression.Property(newExpression.Arguments[0], "Value");
                    var argumentValue2 = Expression.Property(newExpression.Arguments[1], "Value");
                    var argumentValue3 = Expression.Property(newExpression.Arguments[2], "Value");
                    var argumentValue4 = Expression.Property(newExpression.Arguments[3], "Value");
                    var argumentValue5 = Expression.Property(newExpression.Arguments[4], "Value");
                    var argumentValue6 = Expression.Property(newExpression.Arguments[5], "Value");

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        newExpression.Arguments[2],
                        newExpression.Arguments[3],
                        newExpression.Arguments[4],
                        newExpression.Arguments[5],
                        _resultCreatorSixArgs(argumentValue1, argumentValue2, argumentValue3, argumentValue4, argumentValue5, argumentValue6)
                    };

                    return newExpression.Update(newArgs);
                }
            }

            return base.VisitNew(newExpression);
        }
    }

    #endregion

    #region predicate

    private void TestPredicateQuery(
        ISetSource actualSetSource,
        Expression[] roots,
        Expression resultExpression)
    {
        // TODO: fix this
        if (roots.Length == 0)
        {
            return;
        }

        var methodName = roots.Length switch
        {
            1 => nameof(TestPredicateQueryWithOneSourceInternal),
            2 => nameof(TestPredicateQueryWithTwoSourcesInternal),
            3 => nameof(TestPredicateQueryWithThreeSourcesInternal),
            4 => nameof(TestPredicateQueryWithFourSourcesInternal),
            5 => nameof(TestPredicateQueryWithFiveSourcesInternal),
            6 => nameof(TestPredicateQueryWithSixSourcesInternal),
            _ => throw new InvalidOperationException(),
        };

        var method = typeof(OperatorsQueryTestBase).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = method.MakeGenericMethod(roots.Select(x => PropertyTypeToEntityMap[x.Type]).ToArray());

        genericMethod.Invoke(
            this,
            new object[]
            {
                actualSetSource,
                resultExpression,
                roots
            });
    }

    private void TestPredicateQueryWithOneSourceInternal<TEntity1>(
        ISetSource actualSetSource,
        Expression resultExpression,
        Expression[] roots)
        where TEntity1 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            orderby e1.Id
            where DummyTrue(e1)
            select new ValueTuple<TEntity1>(e1);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultExpression, roots);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
        }
    }

    private void TestPredicateQueryWithTwoSourcesInternal<TEntity1, TEntity2>(
        ISetSource actualSetSource,
        Expression resultExpression,
        Expression[] roots)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            orderby e1.Id, e2.Id
            where DummyTrue(e1, e2)
            select new ValueTuple<TEntity1, TEntity2>(e1, e2);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultExpression, roots);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
            Assert.Equal(actualResults[i].Item2.Id, expectedResults[i].Item2.Id);
        }
    }

    private void TestPredicateQueryWithThreeSourcesInternal<TEntity1, TEntity2, TEntity3>(
        ISetSource actualSetSource,
        Expression resultExpression,
        Expression[] roots)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            orderby e1.Id, e2.Id, e3.Id
            where DummyTrue(e1, e2, e3)
            select new ValueTuple<TEntity1, TEntity2, TEntity3>(e1, e2, e3);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultExpression, roots);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
            Assert.Equal(actualResults[i].Item2.Id, expectedResults[i].Item2.Id);
            Assert.Equal(actualResults[i].Item3.Id, expectedResults[i].Item3.Id);
        }
    }

    private void TestPredicateQueryWithFourSourcesInternal<TEntity1, TEntity2, TEntity3, TEntity4>(
        ISetSource actualSetSource,
        Expression resultExpression,
        Expression[] roots)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id
            where DummyTrue(e1, e2, e3, e4)
            select new ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4>(e1, e2, e3, e4);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultExpression, roots);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
            Assert.Equal(actualResults[i].Item2.Id, expectedResults[i].Item2.Id);
            Assert.Equal(actualResults[i].Item3.Id, expectedResults[i].Item3.Id);
            Assert.Equal(actualResults[i].Item4.Id, expectedResults[i].Item4.Id);
        }
    }

    private void TestPredicateQueryWithFiveSourcesInternal<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(
        ISetSource actualSetSource,
        Expression resultExpression,
        Expression[] roots)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            from e5 in ss.Set<TEntity5>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id, e5.Id
            where DummyTrue(e1, e2, e3, e4, e5)
            select new ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(e1, e2, e3, e4, e5);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultExpression, roots);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
            Assert.Equal(actualResults[i].Item2.Id, expectedResults[i].Item2.Id);
            Assert.Equal(actualResults[i].Item3.Id, expectedResults[i].Item3.Id);
            Assert.Equal(actualResults[i].Item4.Id, expectedResults[i].Item4.Id);
            Assert.Equal(actualResults[i].Item5.Id, expectedResults[i].Item5.Id);
        }
    }

    private void TestPredicateQueryWithSixSourcesInternal<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(
        ISetSource actualSetSource,
        Expression resultExpression,
        Expression[] roots)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
        where TEntity6 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            from e5 in ss.Set<TEntity5>()
            from e6 in ss.Set<TEntity6>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id, e5.Id, e6.Id
            where DummyTrue(e1, e2, e3, e4, e5, e6)
            select new ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(e1, e2, e3, e4, e5, e6);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultExpression, roots);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
            Assert.Equal(actualResults[i].Item2.Id, expectedResults[i].Item2.Id);
            Assert.Equal(actualResults[i].Item3.Id, expectedResults[i].Item3.Id);
            Assert.Equal(actualResults[i].Item4.Id, expectedResults[i].Item4.Id);
            Assert.Equal(actualResults[i].Item5.Id, expectedResults[i].Item5.Id);
            Assert.Equal(actualResults[i].Item6.Id, expectedResults[i].Item6.Id);
        }
    }

    private static bool DummyTrue<TEntity1>(TEntity1 e1)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2>(
        TEntity1 e1, TEntity2 e2)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3, TEntity4>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3, TEntity4 e4)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3, TEntity4 e4, TEntity5 e5)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3, TEntity4 e4, TEntity5 e5, TEntity6 e6)
        => true;

    private class ResultExpressionPredicateRewriter : ExpressionVisitor
    {
        private static readonly MethodInfo _likeMethodInfo
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private readonly Expression[] _roots;
        private readonly Expression _resultExpression;

        public ResultExpressionPredicateRewriter(Expression resultExpression, Expression[] roots)
        {
            _resultExpression = resultExpression;
            _roots = roots;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(DummyTrue))
            {
                // replace dummy with the actual predicate
                if (methodCallExpression.Arguments.Count == 1)
                {
                    var replaced = ReplacingExpressionVisitor.Replace(
                        _roots[0],
                        Expression.Property(methodCallExpression.Arguments[0], "Value"),
                        _resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 2)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 3)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 4)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                            Expression.Property(methodCallExpression.Arguments[3], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 5)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                            Expression.Property(methodCallExpression.Arguments[3], "Value"),
                            Expression.Property(methodCallExpression.Arguments[4], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 6)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                            Expression.Property(methodCallExpression.Arguments[3], "Value"),
                            Expression.Property(methodCallExpression.Arguments[4], "Value"),
                            Expression.Property(methodCallExpression.Arguments[5], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    #endregion

    #region model

    protected class OperatorsContext : DbContext
    {
        public OperatorsContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperatorEntityString>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityInt>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityNullableInt>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityLong>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityBool>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityNullableBool>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityDateTimeOffset>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    protected class OperatorsData : ISetSource
    {
        public static readonly OperatorsData Instance = new();

        private readonly List<Expression<Func<string>>> _stringValues = new()
        {
            () => "A",
            () => "B",
            () => "AB",
        };

        private readonly List<Expression<Func<int>>> _intValues = new()
        {
            () => 1,
            () => 2,
            () => 8,
        };

        private readonly List<Expression<Func<int?>>> _nullableIntValues = new()
        {
            () => null,
            () => 2,
            () => 8,
        };

        private readonly List<Expression<Func<long>>> _longValues = new()
        {
            () => 1L,
            () => 2L,
            () => 8L,
        };

        private readonly List<Expression<Func<bool>>> _boolValues = new()
        {
            () => true,
            () => false,
        };

        private readonly List<Expression<Func<bool?>>> _nullableBoolValues = new()
        {
            () => null,
            () => true,
            () => false,
        };

        private readonly List<Expression<Func<DateTimeOffset>>> _dateTimeOffsetValues = new()
        {
            () => new DateTimeOffset(new DateTime(2000, 1, 1, 11, 0, 0), new TimeSpan(5, 10, 0)),
            () => new DateTimeOffset(new DateTime(2000, 1, 1, 10, 0, 0), new TimeSpan(-8, 0, 0)),
            () => new DateTimeOffset(new DateTime(2000, 1, 1, 9, 0, 0), new TimeSpan(13, 0, 0))
        };

        public IReadOnlyList<OperatorEntityString> OperatorEntitiesString { get; }
        public IReadOnlyList<OperatorEntityInt> OperatorEntitiesInt { get; }
        public IReadOnlyList<OperatorEntityNullableInt> OperatorEntitiesNullableInt { get; }
        public IReadOnlyList<OperatorEntityLong> OperatorEntitiesLong { get; }
        public IReadOnlyList<OperatorEntityBool> OperatorEntitiesBool { get; }
        public IReadOnlyList<OperatorEntityNullableBool> OperatorEntitiesNullableBool { get; }
        public IReadOnlyList<OperatorEntityDateTimeOffset> OperatorEntitiesDateTimeOffset { get; }
        public IDictionary<Type, List<Expression>> ConstantExpressionsPerType { get; }

        private OperatorsData()
        {
            OperatorEntitiesString = CreateStrings();
            OperatorEntitiesInt = CreateInts();
            OperatorEntitiesNullableInt = CreateNullableInts();
            OperatorEntitiesLong = CreateLongs();
            OperatorEntitiesBool = CreateBools();
            OperatorEntitiesNullableBool = CreateNullableBools();
            OperatorEntitiesDateTimeOffset = CreateDateTimeOffsets();

            ConstantExpressionsPerType = new Dictionary<Type, List<Expression>>()
            {
                { typeof(string), _stringValues.Select(x => x.Body).ToList() },
                { typeof(int), _intValues.Select(x => x.Body).ToList() },
                { typeof(int?), _nullableIntValues.Select(x => x.Body).ToList() },
                { typeof(long), _longValues.Select(x => x.Body).ToList() },
                { typeof(bool), _boolValues.Select(x => x.Body).ToList() },
                { typeof(bool?), _nullableBoolValues.Select(x => x.Body).ToList() },
                { typeof(DateTimeOffset), _dateTimeOffsetValues.Select(x => x.Body).ToList() },
            };
        }

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(OperatorEntityString))
            {
                return (IQueryable<TEntity>)OperatorEntitiesString.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityInt))
            {
                return (IQueryable<TEntity>)OperatorEntitiesInt.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityNullableInt))
            {
                return (IQueryable<TEntity>)OperatorEntitiesNullableInt.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityLong))
            {
                return (IQueryable<TEntity>)OperatorEntitiesLong.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityBool))
            {
                return (IQueryable<TEntity>)OperatorEntitiesBool.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityNullableBool))
            {
                return (IQueryable<TEntity>)OperatorEntitiesNullableBool.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityDateTimeOffset))
            {
                return (IQueryable<TEntity>)OperatorEntitiesDateTimeOffset.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public IReadOnlyList<OperatorEntityString> CreateStrings()
            => _stringValues.Select((x, i) => new OperatorEntityString { Id = i, Value = _stringValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityInt> CreateInts()
            => _intValues.Select((x, i) => new OperatorEntityInt { Id = i, Value = _intValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityNullableInt> CreateNullableInts()
            => _nullableIntValues.Select((x, i) => new OperatorEntityNullableInt { Id = i, Value = _nullableIntValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityLong> CreateLongs()
            => _longValues.Select((x, i) => new OperatorEntityLong { Id = i, Value = _longValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityBool> CreateBools()
            => _boolValues.Select((x, i) => new OperatorEntityBool { Id = i, Value = _boolValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityNullableBool> CreateNullableBools()
            => _nullableBoolValues.Select((x, i) => new OperatorEntityNullableBool { Id = i, Value = _nullableBoolValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityDateTimeOffset> CreateDateTimeOffsets()
            => _dateTimeOffsetValues.Select((x, i) => new OperatorEntityDateTimeOffset { Id = i, Value = _dateTimeOffsetValues[i].Compile()() }).ToList();
    }

    public abstract class OperatorEntityBase
    {
        public int Id { get; set; }
    }

    public class OperatorEntityString : OperatorEntityBase
    {
        public string Value { get; set; }
    }

    public class OperatorEntityInt : OperatorEntityBase
    {
        public int Value { get; set; }
    }

    public class OperatorEntityNullableInt : OperatorEntityBase
    {
        public int? Value { get; set; }
    }

    public class OperatorEntityLong : OperatorEntityBase
    {
        public long Value { get; set; }
    }

    public class OperatorEntityBool : OperatorEntityBase
    {
        public bool Value { get; set; }
    }

    public class OperatorEntityNullableBool : OperatorEntityBase
    {
        public bool? Value { get; set; }
    }

    public class OperatorEntityDateTimeOffset : OperatorEntityBase
    {
        public DateTimeOffset Value { get; set; }
    }

    #endregion

    public class OperatorDto6<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
        where TEntity6 : OperatorEntityBase
    {
        public OperatorDto6(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4, TEntity5 entity5, TEntity6 entity6, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Entity3 = entity3;
            Entity4 = entity4;
            Entity5 = entity5;
            Entity6 = entity6;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }
        public TEntity3 Entity3 { get; set; }
        public TEntity4 Entity4 { get; set; }
        public TEntity5 Entity5 { get; set; }
        public TEntity6 Entity6 { get; set; }

        public TResult Result { get; set; }
    }

    #region commented


    //[ConditionalFact]
    //public virtual async Task Basic_binary_in_projection()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var binary in Binaries.Where(x => x.ResultType != typeof(bool)))
    //        {
    //            TestProjectionQueryWithTwoSources(
    //                actualSetSource,
    //                binary.InputTypes.Item1,
    //                binary.InputTypes.Item2,
    //                binary.ResultType,
    //                binary.OperatorCreator);
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Basic_binary_in_predicate()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var binary in Binaries.Where(x => x.ResultType == typeof(bool)))
    //        {
    //            TestPredicateQueryWithTwoSources(
    //                actualSetSource,
    //                binary.InputTypes.Item1,
    //                binary.InputTypes.Item2,
    //                binary.OperatorCreator);
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Binary_wrapped_in_unary_in_projection()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var unary in Unaries.Where(x => x.InputType != typeof(bool)))
    //        {
    //            foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
    //            {
    //                var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

    //                TestProjectionQueryWithTwoSources(
    //                    actualSetSource,
    //                    binary.InputTypes.Item1,
    //                    binary.InputTypes.Item2,
    //                    unary.ResultType,
    //                    operatorCreator);
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Binary_wrapped_in_unary_in_predicate()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var unary in Unaries.Where(x => x.InputType == typeof(bool)))
    //        {
    //            foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
    //            {
    //                var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

    //                TestPredicateQueryWithTwoSources(
    //                    actualSetSource,
    //                    binary.InputTypes.Item1,
    //                    binary.InputTypes.Item2,
    //                    operatorCreator);
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Two_unaries_wrapped_in_binary_in_projection()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var binary in Binaries.Where(x => x.ResultType != typeof(bool)))
    //        {
    //            foreach (var unary1 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item1))
    //            {
    //                foreach (var unary2 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item2))
    //                {
    //                    var operatorCreator = (Expression l, Expression r) => binary.OperatorCreator(unary1.OperatorCreator(l), unary2.OperatorCreator(r));

    //                    TestProjectionQueryWithTwoSources(
    //                        actualSetSource,
    //                        unary1.InputType,
    //                        unary2.InputType,
    //                        binary.ResultType,
    //                        operatorCreator);
    //                }
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Two_unaries_wrapped_in_binary_in_predicate()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var binary in Binaries.Where(x => x.ResultType == typeof(bool)))
    //        {
    //            foreach (var unary1 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item1))
    //            {
    //                foreach (var unary2 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item2))
    //                {
    //                    var operatorCreator = (Expression l, Expression r) => binary.OperatorCreator(unary1.OperatorCreator(l), unary2.OperatorCreator(r));

    //                    TestPredicateQueryWithTwoSources(
    //                        actualSetSource,
    //                        unary1.InputType,
    //                        unary2.InputType,
    //                        operatorCreator);
    //                }
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Two_binaries_in_projection1()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var outerBinary in Binaries.Where(x => x.ResultType != typeof(bool)))
    //        {
    //            foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
    //            {
    //                var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(innerBinary.OperatorCreator(f, s), t);

    //                TestProjectionQueryWithThreeSources(
    //                    actualSetSource,
    //                    innerBinary.InputTypes.Item1,
    //                    innerBinary.InputTypes.Item2,
    //                    outerBinary.InputTypes.Item2,
    //                    outerBinary.ResultType,
    //                    operatorCreator);
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Two_binaries_in_projection2()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var outerBinary in Binaries.Where(x => x.ResultType != typeof(bool)))
    //        {
    //            foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2))
    //            {
    //                var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(f, innerBinary.OperatorCreator(s, t));

    //                // avoid divide by 0 exception
    //                if (innerBinary.OperatorCreator.Method.Name == nameof(Expression.Subtract)
    //                    || innerBinary.OperatorCreator.Method.Name == nameof(Expression.Modulo))
    //                {
    //                    continue;
    //                }

    //                TestProjectionQueryWithThreeSources(
    //                    actualSetSource,
    //                    outerBinary.InputTypes.Item1,
    //                    innerBinary.InputTypes.Item1,
    //                    innerBinary.InputTypes.Item2,
    //                    outerBinary.ResultType,
    //                    operatorCreator);
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Two_binaries_in_predicate1()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
    //        {
    //            foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
    //            {
    //                var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(innerBinary.OperatorCreator(f, s), t);

    //                TestPredicateQueryWithThreeSources(
    //                    actualSetSource,
    //                    innerBinary.InputTypes.Item1,
    //                    innerBinary.InputTypes.Item2,
    //                    outerBinary.InputTypes.Item2,
    //                    operatorCreator);
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Two_binaries_in_predicate2()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
    //        {
    //            foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2))
    //            {
    //                var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(f, innerBinary.OperatorCreator(s, t));

    //                TestPredicateQueryWithThreeSources(
    //                    actualSetSource,
    //                    outerBinary.InputTypes.Item1,
    //                    innerBinary.InputTypes.Item1,
    //                    innerBinary.InputTypes.Item2,
    //                    operatorCreator);
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Basic_binary_wrapped_in_unary_in_projection()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var unary in Unaries.Where(x => x.InputType != typeof(bool)))
    //        {
    //            foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
    //            {
    //                var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

    //                TestProjectionQueryWithTwoSources(
    //                    actualSetSource,
    //                    binary.InputTypes.Item1,
    //                    binary.InputTypes.Item2,
    //                    unary.ResultType,
    //                    operatorCreator);
    //            }
    //        }
    //    }
    //}

    //[ConditionalFact]
    //public virtual async Task Fubar_in_predicate()
    //{
    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
    //        {
    //            foreach (var leftBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
    //            {
    //                foreach (var rightBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2
    //                    && x.InputTypes.Item1 == leftBinary.InputTypes.Item2))
    //                {
    //                    // re-use same type to prevent result explosion
    //                    var operatorCreator = (Expression l, Expression m, Expression r) => outerBinary.OperatorCreator(
    //                        leftBinary.OperatorCreator(l, m), rightBinary.OperatorCreator(m, r));

    //                    TestPredicateQueryWithThreeSources(
    //                        actualSetSource,
    //                        leftBinary.InputTypes.Item1,
    //                        leftBinary.InputTypes.Item2,
    //                        rightBinary.InputTypes.Item2,
    //                        operatorCreator);
    //                }
    //            }
    //        }
    //    }
    //}




    //[ConditionalFact]
    //public virtual async Task This_is_what_it_takes()
    //{
    //    Expression<Func<DateTimeOffset>> kupson = () => new DateTimeOffset(new DateTime(2000, 1, 1, 11, 0, 0), new TimeSpan(5, 10, 0));



    //    var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
    //    using (var context = contextFactory.CreateContext())
    //    {
    //        var actualSetSource = new ActualSetSource(context);
    //        foreach (var outermostBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
    //        {
    //            foreach (var outerBinary in Binaries.Where(x => x.ResultType == outermostBinary.InputTypes.Item1))
    //            {
    //                foreach (var leftBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
    //                {
    //                    foreach (var leftBinaryUnary1 in Unaries.Where(x => x.ResultType == leftBinary.InputTypes.Item1))
    //                    {
    //                        foreach (var leftBinaryUnary2 in Unaries.Where(x => x.ResultType == leftBinary.InputTypes.Item2))
    //                        {
    //                            foreach (var rightBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2))
    //                            {
    //                                foreach (var rightBinaryUnary1 in Unaries.Where(x => x.ResultType == rightBinary.InputTypes.Item1
    //                                    && x.InputType == leftBinaryUnary1.InputType))
    //                                {
    //                                    foreach (var rightBinaryUnary2 in Unaries.Where(x => x.ResultType == rightBinary.InputTypes.Item2
    //                                        && x.InputType == leftBinaryUnary2.InputType))
    //                                    {
    //                                        foreach (var outerUnary in Unaries.Where(x => x.ResultType == outermostBinary.InputTypes.Item2))
    //                                        {
    //                                            var operatorCreator = (Expression l, Expression m, Expression r) => outermostBinary.OperatorCreator(
    //                                                outerBinary.OperatorCreator(
    //                                                    leftBinary.OperatorCreator(
    //                                                        leftBinaryUnary1.OperatorCreator(l), leftBinaryUnary2.OperatorCreator(m)),
    //                                                    rightBinary.OperatorCreator(
    //                                                        rightBinaryUnary1.OperatorCreator(l), rightBinaryUnary2.OperatorCreator(m))),
    //                                                outerUnary.OperatorCreator(r));

    //                                            TestPredicateQueryWithThreeSources(
    //                                                actualSetSource,
    //                                                leftBinaryUnary1.InputType,
    //                                                leftBinaryUnary2.InputType,
    //                                                outerUnary.InputType,
    //                                                operatorCreator);
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}




    //public class OperatorDto2<TEntity1, TEntity2, TResult>
    //    where TEntity1 : OperatorEntityBase
    //    where TEntity2 : OperatorEntityBase
    //{
    //    public OperatorDto2(TEntity1 entity1, TEntity2 entity2, TResult result)
    //    {
    //        Entity1 = entity1;
    //        Entity2 = entity2;
    //        Result = result;
    //    }

    //    public TEntity1 Entity1 { get; set; }
    //    public TEntity2 Entity2 { get; set; }

    //    public TResult Result { get; set; }
    //}

    //public class OperatorDto3<TEntity1, TEntity2, TEntity3, TResult>
    //    where TEntity1 : OperatorEntityBase
    //    where TEntity2 : OperatorEntityBase
    //    where TEntity3 : OperatorEntityBase
    //{
    //    public OperatorDto3(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TResult result)
    //    {
    //        Entity1 = entity1;
    //        Entity2 = entity2;
    //        Entity3 = entity3;
    //        Result = result;
    //    }

    //    public TEntity1 Entity1 { get; set; }
    //    public TEntity2 Entity2 { get; set; }
    //    public TEntity3 Entity3 { get; set; }

    //    public TResult Result { get; set; }
    //}

    #endregion
}
