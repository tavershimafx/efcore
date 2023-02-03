// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    public virtual async Task Procedural_predicate_six_sources_random()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var seed = new Random().Next();
            var random = new Random(seed);
            var maxDepth = 10;

            var possibleTypes = OperatorsData.Instance.ConstantExpressionsPerType.Keys.ToArray();

            var types = new Type[6];
            for (var i = 0; i < types.Length; i++)
            {
                types[i] = possibleTypes[random.Next(possibleTypes.Length)];
                types[i + 1] = types[i];
                i++;
            }

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

            var currentResultType = typeof(bool);




            // if not recursion - input types must be present
            // if recursion 






            var depth = 0;
            while (depth <= maxDepth)
            {
                // 0 - binary, 1 - unary, 2 - binary with constant
                var operationType = random.Next(3);

                if (operationType == 2)
                {
                    var type = types[random.Next(types.Length)];
                    //var possibleOperations = Binaries.Where(x => x.ResultType == currentResultType)



                }







                // probability of nesting depends on current depth - the deeper we get, the lower the probability
                if (operationType != 2)
                {
                    var shouldNest = random.Next(maxDepth);
                    if (shouldNest > depth)
                    {

                    }

                }
            }
        }
    }

    protected class ExpectedQueryRewritingVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(
                nameof(string.StartsWith), new[] { typeof(string) })!;

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

                return Expression.Equal(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2]);

            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    #region projection

    //private void TestProjectionQueryWithTwoSources(
    //    ISetSource actualSetSource,
    //    Type firstType,
    //    Type secondType,
    //    Type resultType,
    //    Func<Expression, Expression, Expression> resultCreator)
    //{
    //    var method = typeof(OperatorsQueryTestBase).GetMethod(
    //        nameof(TestProjectionQueryWithTwoSourcesInternal),
    //        BindingFlags.NonPublic | BindingFlags.Instance);

    //    var genericMethod = method.MakeGenericMethod(
    //        PropertyTypeToEntityMap[firstType],
    //        PropertyTypeToEntityMap[secondType],
    //        resultType);

    //    genericMethod.Invoke(
    //        this,
    //        new object[]
    //        {
    //            actualSetSource,
    //            resultCreator
    //        });
    //}

    //private void TestProjectionQueryWithThreeSources(
    //    ISetSource actualSetSource,
    //    Type firstType,
    //    Type secondType,
    //    Type thirdType,
    //    Type resultType,
    //    Func<Expression, Expression, Expression, Expression> resultCreator)
    //{
    //    var method = typeof(OperatorsQueryTestBase).GetMethod(
    //        nameof(TestProjectionQueryWithThreeSourcesInternal),
    //        BindingFlags.NonPublic | BindingFlags.Instance);

    //    var genericMethod = method.MakeGenericMethod(
    //        PropertyTypeToEntityMap[firstType],
    //        PropertyTypeToEntityMap[secondType],
    //        PropertyTypeToEntityMap[thirdType],
    //        resultType);

    //    genericMethod.Invoke(
    //        this,
    //        new object[]
    //        {
    //            actualSetSource,
    //            resultCreator
    //        });
    //}

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

    //private void TestProjectionQueryWithTwoSourcesInternal<TFirst, TSecond, TResult>(
    //    ISetSource actualSetSource,
    //    Func<Expression, Expression, Expression> resultCreator)
    //    where TFirst : OperatorEntityBase
    //    where TSecond : OperatorEntityBase
    //{
    //    var setSourceTemplate = (ISetSource ss) =>
    //        from e1 in ss.Set<TFirst>()
    //        from e2 in ss.Set<TSecond>()
    //        orderby e1.Id, e2.Id
    //        select new OperatorDto2<TFirst, TSecond, TResult>(e1, e2, default);

    //    var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
    //    var actualQueryTemplate = setSourceTemplate(actualSetSource);
    //    var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
    //    var actualQuery = actualQueryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(actualRewritten);
    //    var actualResults = actualQuery.ToList();

    //    var expectedQueryTemplate = setSourceTemplate(ExpectedData);
    //    var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
    //    expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
    //    var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(expectedRewritten);
    //    var expectedResults = expectedQuery.ToList();

    //    Assert.Equal(actualResults.Count, expectedResults.Count);
    //    for (var i = 0; i < actualResults.Count; i++)
    //    {
    //        Assert.Equal(actualResults[i].Result, expectedResults[i].Result);
    //    }
    //}

    //private void TestProjectionQueryWithThreeSourcesInternal<TFirst, TSecond, TThird, TResult>(
    //    ISetSource actualSetSource,
    //    Func<Expression, Expression, Expression, Expression> resultCreator)
    //    where TFirst : OperatorEntityBase
    //    where TSecond : OperatorEntityBase
    //    where TThird : OperatorEntityBase
    //{
    //    var setSourceTemplate = (ISetSource ss) =>
    //        from e1 in ss.Set<TFirst>()
    //        from e2 in ss.Set<TSecond>()
    //        from e3 in ss.Set<TThird>()
    //        orderby e1.Id, e2.Id, e3.Id
    //        select new OperatorDto3<TFirst, TSecond, TThird, TResult>(e1, e2, e3, default);

    //    var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
    //    var actualQueryTemplate = setSourceTemplate(actualSetSource);
    //    var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
    //    var actualQuery = actualQueryTemplate.Provider.CreateQuery<OperatorDto3<TFirst, TSecond, TThird, TResult>>(actualRewritten);
    //    var actualResults = actualQuery.ToList();

    //    var expectedQueryTemplate = setSourceTemplate(ExpectedData);
    //    var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
    //    expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
    //    var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<OperatorDto3<TFirst, TSecond, TThird, TResult>>(expectedRewritten);
    //    var expectedResults = expectedQuery.ToList();

    //    Assert.Equal(actualResults.Count, expectedResults.Count);
    //    for (var i = 0; i < actualResults.Count; i++)
    //    {
    //        Assert.Equal(actualResults[i].Result, expectedResults[i].Result);
    //    }
    //}

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
        //private readonly Func<Expression, Expression, Expression> _resultCreatorTwoArgs;
        //private readonly Func<Expression, Expression, Expression, Expression> _resultCreatorThreeArgs;
        private readonly Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> _resultCreatorSixArgs;

        //public ResultExpressionProjectionRewriter(Func<Expression, Expression, Expression> resultCreatorTwoArgs)
        //{
        //    _resultCreatorTwoArgs = resultCreatorTwoArgs;
        //}

        //public ResultExpressionProjectionRewriter(Func<Expression, Expression, Expression, Expression> resultCreatorThreeArgs)
        //{
        //    _resultCreatorThreeArgs = resultCreatorThreeArgs;
        //}

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

                //if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto2<,,>))
                //{
                //    var firstArgumentValue = Expression.Property(newExpression.Arguments[0], "Value");
                //    var secondArgumentValue = Expression.Property(newExpression.Arguments[1], "Value");

                //    var newArgs = new List<Expression>
                //    {
                //        newExpression.Arguments[0],
                //        newExpression.Arguments[1],
                //        _resultCreatorTwoArgs(firstArgumentValue, secondArgumentValue)
                //    };

                //    return newExpression.Update(newArgs);
                //}

                //if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto3<,,,>))
                //{
                //    var firstArgumentValue = Expression.Property(newExpression.Arguments[0], "Value");
                //    var secondArgumentValue = Expression.Property(newExpression.Arguments[1], "Value");
                //    var thirdArgumentValue = Expression.Property(newExpression.Arguments[1], "Value");

                //    var newArgs = new List<Expression>
                //    {
                //        newExpression.Arguments[0],
                //        newExpression.Arguments[1],
                //        newExpression.Arguments[2],
                //        _resultCreatorThreeArgs(firstArgumentValue, secondArgumentValue, thirdArgumentValue)
                //    };

                //    return newExpression.Update(newArgs);
                //}
            }

            return base.VisitNew(newExpression);
        }
    }

    #endregion

    #region predicate

    private void TestPredicateQueryWithTwoSources(
        ISetSource actualSetSource,
        Type firstType,
        Type secondType,
        Func<Expression, Expression, Expression> resultCreator)
    {
        var method = typeof(OperatorsQueryTestBase).GetMethod(
            nameof(TestPredicateQueryWithTwoSourcesInternal),
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = method.MakeGenericMethod(
            PropertyTypeToEntityMap[firstType],
            PropertyTypeToEntityMap[secondType]);

        genericMethod.Invoke(
            this,
            new object[]
            {
                actualSetSource,
                resultCreator
            });
    }

    private void TestPredicateQueryWithThreeSources(
        ISetSource actualSetSource,
        Type firstType,
        Type secondType,
        Type thirdType,
        Func<Expression, Expression, Expression, Expression> resultCreator)
    {
        var method = typeof(OperatorsQueryTestBase).GetMethod(
            nameof(TestPredicateQueryWithThreeSourcesInternal),
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = method.MakeGenericMethod(
            PropertyTypeToEntityMap[firstType],
            PropertyTypeToEntityMap[secondType],
            PropertyTypeToEntityMap[thirdType]);

        genericMethod.Invoke(
            this,
            new object[]
            {
                actualSetSource,
                resultCreator
            });
    }
    private void TestPredicateQueryWithSixSources(
            ISetSource actualSetSource,
            Type type1,
            Type type2,
            Type type3,
            Type type4,
            Type type5,
            Type type6,
            Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> resultCreator)
        {
            var method = typeof(OperatorsQueryTestBase).GetMethod(
                nameof(TestPredicateQueryWithSixSourcesInternal),
                BindingFlags.NonPublic | BindingFlags.Instance);

            var genericMethod = method.MakeGenericMethod(
                PropertyTypeToEntityMap[type1],
                PropertyTypeToEntityMap[type2],
                PropertyTypeToEntityMap[type3],
                PropertyTypeToEntityMap[type4],
                PropertyTypeToEntityMap[type5],
                PropertyTypeToEntityMap[type6]);

            genericMethod.Invoke(
                this,
                new object[]
                {
                    actualSetSource,
                    resultCreator
                });
        }

    private void TestPredicateQueryWithTwoSourcesInternal<TFirst, TSecond>(
        ISetSource actualSetSource,
        Func<Expression, Expression, Expression> resultCreator)
        where TFirst : OperatorEntityBase
        where TSecond : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TFirst>()
            from e2 in ss.Set<TSecond>()
            orderby e1.Id, e2.Id
            where DummyTrue(e1, e2)
            select new ValueTuple<TFirst, TSecond>(e1, e2);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultCreator);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TFirst, TSecond>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TFirst, TSecond>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
            Assert.Equal(actualResults[i].Item2.Id, expectedResults[i].Item2.Id);
        }
    }

    private void TestPredicateQueryWithThreeSourcesInternal<TFirst, TSecond, TThird>(
        ISetSource actualSetSource,
        Func<Expression, Expression, Expression, Expression> resultCreator)
        where TFirst : OperatorEntityBase
        where TSecond : OperatorEntityBase
        where TThird : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TFirst>()
            from e2 in ss.Set<TSecond>()
            from e3 in ss.Set<TThird>()
            orderby e1.Id, e2.Id, e3.Id
            where DummyTrue(e1, e2, e3)
            select new ValueTuple<TFirst, TSecond, TThird>(e1, e2, e3);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultCreator);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<ValueTuple<TFirst, TSecond, TThird>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<ValueTuple<TFirst, TSecond, TThird>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Item1.Id, expectedResults[i].Item1.Id);
            Assert.Equal(actualResults[i].Item2.Id, expectedResults[i].Item2.Id);
            Assert.Equal(actualResults[i].Item3.Id, expectedResults[i].Item3.Id);
        }
    }

    private void TestPredicateQueryWithSixSourcesInternal<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(
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
            where DummyTrue(e1, e2, e3, e4, e5, e6)
            select new ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(e1, e2, e3, e4, e5, e6);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultCreator);
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
        }
    }

    private static bool DummyTrue<TFirst, TSecond>(TFirst first, TSecond second)
        => true;

    private static bool DummyTrue<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3, TEntity4 e4, TEntity5 e5, TEntity6 e6)
        => true;

    private class ResultExpressionPredicateRewriter : ExpressionVisitor
    {
        private readonly Func<Expression, Expression, Expression> _resultCreatorTwoArgs;
        private readonly Func<Expression, Expression, Expression, Expression> _resultCreatorThreeArgs;
        private readonly Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> _resultCreatorSixArgs;

        private static readonly MethodInfo _likeMethodInfo
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        public ResultExpressionPredicateRewriter(Func<Expression, Expression, Expression> resultCreatorTwoArgs)
        {
            _resultCreatorTwoArgs = resultCreatorTwoArgs;
        }

        public ResultExpressionPredicateRewriter(Func<Expression, Expression, Expression, Expression> resultCreatorThreeArgs)
        {
            _resultCreatorThreeArgs = resultCreatorThreeArgs;
        }

        public ResultExpressionPredicateRewriter(Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> resultCreatorSixArgs)
        {
            _resultCreatorSixArgs = resultCreatorSixArgs;
        }


        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(DummyTrue))
            {
                // replace dummy with the actual predicate
                if (methodCallExpression.Arguments.Count == 2)
                {
                    var firstArgumentValue = Expression.Property(methodCallExpression.Arguments[0], "Value");
                    var secondArgumentValue = Expression.Property(methodCallExpression.Arguments[1], "Value");

                    return _resultCreatorTwoArgs(firstArgumentValue, secondArgumentValue);
                }

                if (methodCallExpression.Arguments.Count == 3)
                {
                    var firstArgumentValue = Expression.Property(methodCallExpression.Arguments[0], "Value");
                    var secondArgumentValue = Expression.Property(methodCallExpression.Arguments[1], "Value");
                    var thirdArgumentValue = Expression.Property(methodCallExpression.Arguments[2], "Value");

                    return _resultCreatorThreeArgs(firstArgumentValue, secondArgumentValue, thirdArgumentValue);
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
