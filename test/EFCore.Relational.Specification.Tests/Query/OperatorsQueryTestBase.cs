// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OperatorsQueryTestBase : NonSharedModelTestBase
{
    protected readonly List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> Binaries;
    protected readonly List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> Unaries;
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
            //((typeof(int), typeof(int)), typeof(int), Expression.Multiply),
            //((typeof(int), typeof(int)), typeof(int), Expression.Divide),
            //((typeof(int), typeof(int)), typeof(int), Expression.Modulo),
            //((typeof(int), typeof(int)), typeof(int), Expression.Add),
            //((typeof(int), typeof(int)), typeof(int), Expression.Subtract),


            ((typeof(string), typeof(string)), typeof(string), (x, y) => Expression.Add(x, y, _stringConcatMethodInfo)),

            ((typeof(bool), typeof(bool)), typeof(bool), Expression.And),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Or),

            //((typeof(int), typeof(int)), typeof(int), Expression.LeftShift),
            //((typeof(int), typeof(int)), typeof(int), Expression.RightShift),

            //((typeof(int), typeof(int)), typeof(bool), Expression.LessThan),
            //((typeof(int), typeof(int)), typeof(bool), Expression.LessThanOrEqual),
            //((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThan),
            //((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThanOrEqual),

            ((typeof(int), typeof(int)), typeof(bool), Expression.Equal),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Equal),
            ((typeof(string), typeof(string)), typeof(bool), Expression.Equal),

            ((typeof(int), typeof(int)), typeof(bool), Expression.NotEqual),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.NotEqual),
            ((typeof(string), typeof(string)), typeof(bool), Expression.NotEqual),

            //((typeof(bool), typeof(bool)), typeof(bool), Expression.AndAlso),
            //((typeof(bool), typeof(bool)), typeof(bool), Expression.OrElse),

            //((typeof(string), typeof(string)), typeof(bool), (x, y) => Expression.Call(
            //    null,
            //    _likeMethodInfo,
            //    Expression.Constant(EF.Functions),
            //    x,
            //    y)),
        };

        Unaries = new()
        {
            //(typeof(bool), typeof(bool), Expression.Not),
            //(typeof(int), typeof(int), Expression.Not),
            //(typeof(int), typeof(int), Expression.Negate),

            //(typeof(bool?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(bool?)))),
            //(typeof(int?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(int?)))),
            //(typeof(string), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(string)))),

            //(typeof(bool?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(bool?)))),
            //(typeof(int?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(int?)))),
            //(typeof(string), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(string)))),

            //(typeof(string), typeof(bool), x => Expression.Call(
            //    null,
            //    _likeMethodInfo,
            //    Expression.Constant(EF.Functions),
            //    x,
            //    Expression.Constant("A%"))),

            (typeof(string), typeof(bool), x => Expression.Equal(x, Expression.Constant("A", typeof(string)))),
            (typeof(string), typeof(bool), x => Expression.Equal(x, Expression.Constant("B", typeof(string)))),
            (typeof(string), typeof(bool), x => Expression.NotEqual(x, Expression.Constant("AB", typeof(string)))),

            (typeof(int), typeof(bool), x => Expression.Equal(x, Expression.Constant(1, typeof(int)))),
            (typeof(int), typeof(bool), x => Expression.Equal(x, Expression.Constant(2, typeof(int)))),
            (typeof(int), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(5, typeof(int)))),
        };

        PropertyTypeToEntityMap = new()
        {
            { typeof(string), typeof(OperatorEntityString) },
            { typeof(int), typeof(OperatorEntityInt) },
            { typeof(int?), typeof(OperatorEntityNullableInt) },
            { typeof(bool), typeof(OperatorEntityBool) },
            { typeof(bool?), typeof(OperatorEntityNullableBool) },
            { typeof(DateTimeOffset), typeof(OperatorEntityDateTimeOffset) },
        };

        ExpectedData = OperatorsData.Instance;
        ExpectedQueryRewriter = new ExpectedQueryRewritingVisitor();
    }

    protected override string StoreName
        => "OperatorsTest";

    //protected abstract void Seed(OperatorsContext ctx);
    protected virtual void Seed(OperatorsContext ctx)
    {
        ctx.Set<OperatorEntityString>().AddRange(ExpectedData.OperatorEntitiesString);
        ctx.Set<OperatorEntityInt>().AddRange(ExpectedData.OperatorEntitiesInt);
        ctx.Set<OperatorEntityNullableInt>().AddRange(ExpectedData.OperatorEntitiesNullableInt);
        ctx.Set<OperatorEntityBool>().AddRange(ExpectedData.OperatorEntitiesBool);
        ctx.Set<OperatorEntityNullableBool>().AddRange(ExpectedData.OperatorEntitiesNullableBool);
        ctx.Set<OperatorEntityDateTimeOffset>().AddRange(ExpectedData.OperatorEntitiesDateTimeOffset);

        ctx.SaveChanges();
    }

    [ConditionalFact]
    public virtual async Task Basic_binary_in_projection()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var binary in Binaries.Where(x => x.ResultType != typeof(bool)))
            {
                TestProjectionQueryWithTwoSources(
                    actualSetSource,
                    binary.InputTypes.Item1,
                    binary.InputTypes.Item2,
                    binary.ResultType,
                    binary.OperatorCreator);
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Basic_binary_in_predicate()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var binary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                TestPredicateQueryWithTwoSources(
                    actualSetSource,
                    binary.InputTypes.Item1,
                    binary.InputTypes.Item2,
                    binary.OperatorCreator);
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Binary_wrapped_in_unary_in_projection()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var unary in Unaries.Where(x => x.InputType != typeof(bool)))
            {
                foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
                {
                    var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

                    TestProjectionQueryWithTwoSources(
                        actualSetSource,
                        binary.InputTypes.Item1,
                        binary.InputTypes.Item2,
                        unary.ResultType,
                        operatorCreator);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Binary_wrapped_in_unary_in_predicate()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var unary in Unaries.Where(x => x.InputType == typeof(bool)))
            {
                foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
                {
                    var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

                    TestPredicateQueryWithTwoSources(
                        actualSetSource,
                        binary.InputTypes.Item1,
                        binary.InputTypes.Item2,
                        operatorCreator);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Two_unaries_wrapped_in_binary_in_projection()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var binary in Binaries.Where(x => x.ResultType != typeof(bool)))
            {
                foreach (var unary1 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item1))
                {
                    foreach (var unary2 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item2))
                    {
                        var operatorCreator = (Expression l, Expression r) => binary.OperatorCreator(unary1.OperatorCreator(l), unary2.OperatorCreator(r));

                        TestProjectionQueryWithTwoSources(
                            actualSetSource,
                            unary1.InputType,
                            unary2.InputType,
                            binary.ResultType,
                            operatorCreator);
                    }
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Two_unaries_wrapped_in_binary_in_predicate()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var binary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var unary1 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item1))
                {
                    foreach (var unary2 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item2))
                    {
                        var operatorCreator = (Expression l, Expression r) => binary.OperatorCreator(unary1.OperatorCreator(l), unary2.OperatorCreator(r));

                        TestPredicateQueryWithTwoSources(
                            actualSetSource,
                            unary1.InputType,
                            unary2.InputType,
                            operatorCreator);
                    }
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Two_binaries_in_projection1()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var outerBinary in Binaries.Where(x => x.ResultType != typeof(bool)))
            {
                foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
                {
                    var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(innerBinary.OperatorCreator(f, s), t);

                    TestProjectionQueryWithThreeSources(
                        actualSetSource,
                        innerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item2,
                        outerBinary.InputTypes.Item2,
                        outerBinary.ResultType,
                        operatorCreator);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Two_binaries_in_projection2()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var outerBinary in Binaries.Where(x => x.ResultType != typeof(bool)))
            {
                foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2))
                {
                    var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(f, innerBinary.OperatorCreator(s, t));

                    // avoid divide by 0 exception
                    if (innerBinary.OperatorCreator.Method.Name == nameof(Expression.Subtract)
                        || innerBinary.OperatorCreator.Method.Name == nameof(Expression.Modulo))
                    {
                        continue;
                    }

                    TestProjectionQueryWithThreeSources(
                        actualSetSource,
                        outerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item2,
                        outerBinary.ResultType,
                        operatorCreator);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Two_binaries_in_predicate1()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
                {
                    var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(innerBinary.OperatorCreator(f, s), t);

                    TestPredicateQueryWithThreeSources(
                        actualSetSource,
                        innerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item2,
                        outerBinary.InputTypes.Item2,
                        operatorCreator);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Two_binaries_in_predicate2()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2))
                {
                    var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(f, innerBinary.OperatorCreator(s, t));

                    TestPredicateQueryWithThreeSources(
                        actualSetSource,
                        outerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item2,
                        operatorCreator);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Basic_binary_wrapped_in_unary_in_projection()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var unary in Unaries.Where(x => x.InputType != typeof(bool)))
            {
                foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
                {
                    var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

                    TestProjectionQueryWithTwoSources(
                        actualSetSource,
                        binary.InputTypes.Item1,
                        binary.InputTypes.Item2,
                        unary.ResultType,
                        operatorCreator);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Fubar_in_predicate()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var leftBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
                {
                    foreach (var rightBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2
                        && x.InputTypes.Item1 == leftBinary.InputTypes.Item2))
                    {
                        // re-use same type to prevent result explosion
                        var operatorCreator = (Expression l, Expression m, Expression r) => outerBinary.OperatorCreator(
                            leftBinary.OperatorCreator(l, m), rightBinary.OperatorCreator(m, r));

                        TestPredicateQueryWithThreeSources(
                            actualSetSource,
                            leftBinary.InputTypes.Item1,
                            leftBinary.InputTypes.Item2,
                            rightBinary.InputTypes.Item2,
                            operatorCreator);
                    }
                }
            }
        }
    }




    [ConditionalFact]
    public virtual async Task This_is_what_it_takes()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);
            foreach (var outermostBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var outerBinary in Binaries.Where(x => x.ResultType == outermostBinary.InputTypes.Item1))
                {
                    foreach (var leftBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
                    {
                        foreach (var leftBinaryUnary1 in Unaries.Where(x => x.ResultType == leftBinary.InputTypes.Item1))
                        {
                            foreach (var leftBinaryUnary2 in Unaries.Where(x => x.ResultType == leftBinary.InputTypes.Item2))
                            {
                                foreach (var rightBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2))
                                {
                                    foreach (var rightBinaryUnary1 in Unaries.Where(x => x.ResultType == rightBinary.InputTypes.Item1
                                        && x.InputType == leftBinaryUnary1.InputType))
                                    {
                                        foreach (var rightBinaryUnary2 in Unaries.Where(x => x.ResultType == rightBinary.InputTypes.Item2
                                            && x.InputType == leftBinaryUnary2.InputType))
                                        {
                                            foreach (var outerUnary in Unaries.Where(x => x.ResultType == outermostBinary.InputTypes.Item2))
                                            {
                                                var operatorCreator = (Expression l, Expression m, Expression r) => outermostBinary.OperatorCreator(
                                                    outerBinary.OperatorCreator(
                                                        leftBinary.OperatorCreator(
                                                            leftBinaryUnary1.OperatorCreator(l), leftBinaryUnary2.OperatorCreator(m)),
                                                        rightBinary.OperatorCreator(
                                                            rightBinaryUnary1.OperatorCreator(l), rightBinaryUnary2.OperatorCreator(m))),
                                                    outerUnary.OperatorCreator(r));

                                                TestPredicateQueryWithThreeSources(
                                                    actualSetSource,
                                                    leftBinaryUnary1.InputType,
                                                    leftBinaryUnary2.InputType,
                                                    outerUnary.InputType,
                                                    operatorCreator);
                                            }
                                        }
                                    }
                                }
                            }
                        }
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

    private void TestProjectionQueryWithTwoSources(
        ISetSource actualSetSource,
        Type firstType,
        Type secondType,
        Type resultType,
        Func<Expression, Expression, Expression> resultCreator)
    {
        var method = typeof(OperatorsQueryTestBase).GetMethod(
            nameof(TestProjectionQueryWithTwoSourcesInternal),
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = method.MakeGenericMethod(
            PropertyTypeToEntityMap[firstType],
            PropertyTypeToEntityMap[secondType],
            resultType);

        genericMethod.Invoke(
            this,
            new object[]
            {
                actualSetSource,
                resultCreator
            });
    }

    private void TestProjectionQueryWithThreeSources(
        ISetSource actualSetSource,
        Type firstType,
        Type secondType,
        Type thirdType,
        Type resultType,
        Func<Expression, Expression, Expression, Expression> resultCreator)
    {
        var method = typeof(OperatorsQueryTestBase).GetMethod(
            nameof(TestProjectionQueryWithThreeSourcesInternal),
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = method.MakeGenericMethod(
            PropertyTypeToEntityMap[firstType],
            PropertyTypeToEntityMap[secondType],
            PropertyTypeToEntityMap[thirdType],
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

    private void TestProjectionQueryWithTwoSourcesInternal<TFirst, TSecond, TResult>(
        ISetSource actualSetSource,
        Func<Expression, Expression, Expression> resultCreator)
        where TFirst : OperatorEntityBase
        where TSecond : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TFirst>()
            from e2 in ss.Set<TSecond>()
            orderby e1.Id, e2.Id
            select new OperatorDto2<TFirst, TSecond, TResult>(e1, e2, default);

        var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Result, expectedResults[i].Result);
        }
    }

    private void TestProjectionQueryWithThreeSourcesInternal<TFirst, TSecond, TThird, TResult>(
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
            select new OperatorDto3<TFirst, TSecond, TThird, TResult>(e1, e2, e3, default);

        var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<OperatorDto3<TFirst, TSecond, TThird, TResult>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<OperatorDto3<TFirst, TSecond, TThird, TResult>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();

        Assert.Equal(actualResults.Count, expectedResults.Count);
        for (var i = 0; i < actualResults.Count; i++)
        {
            Assert.Equal(actualResults[i].Result, expectedResults[i].Result);
        }
    }

    private class ResultExpressionProjectionRewriter : ExpressionVisitor
    {
        private readonly Func<Expression, Expression, Expression> _resultCreatorTwoArgs;
        private readonly Func<Expression, Expression, Expression, Expression> _resultCreatorThreeArgs;

        public ResultExpressionProjectionRewriter(Func<Expression, Expression, Expression> resultCreatorTwoArgs)
        {
            _resultCreatorTwoArgs = resultCreatorTwoArgs;
        }

        public ResultExpressionProjectionRewriter(Func<Expression, Expression, Expression, Expression> resultCreatorThreeArgs)
        {
            _resultCreatorThreeArgs = resultCreatorThreeArgs;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            if (newExpression.Constructor is ConstructorInfo ctorInfo
                && ctorInfo.DeclaringType is Type { IsGenericType: true } declaringType)
            {
                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto2<,,>))
                {
                    var firstArgumentValue = Expression.Property(newExpression.Arguments[0], "Value");
                    var secondArgumentValue = Expression.Property(newExpression.Arguments[1], "Value");

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        _resultCreatorTwoArgs(firstArgumentValue, secondArgumentValue)
                    };

                    return newExpression.Update(newArgs);
                }

                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto3<,,,>))
                {
                    var firstArgumentValue = Expression.Property(newExpression.Arguments[0], "Value");
                    var secondArgumentValue = Expression.Property(newExpression.Arguments[1], "Value");
                    var thirdArgumentValue = Expression.Property(newExpression.Arguments[1], "Value");

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        newExpression.Arguments[2],
                        _resultCreatorThreeArgs(firstArgumentValue, secondArgumentValue, thirdArgumentValue)
                    };

                    return newExpression.Update(newArgs);
                }
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

    private static bool DummyTrue<TFirst, TSecond>(TFirst first, TSecond second)
        => true;

    private static bool DummyTrue<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third)
        => true;

    private class ResultExpressionPredicateRewriter : ExpressionVisitor
    {
        private readonly Func<Expression, Expression, Expression> _resultCreatorTwoArgs;
        private readonly Func<Expression, Expression, Expression, Expression> _resultCreatorThreeArgs;

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
            modelBuilder.Entity<OperatorEntityBool>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityNullableBool>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityDateTimeOffset>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    protected class OperatorsData : ISetSource
    {
        public static readonly OperatorsData Instance = new();

        public IReadOnlyList<OperatorEntityString> OperatorEntitiesString { get; }
        public IReadOnlyList<OperatorEntityInt> OperatorEntitiesInt { get; }
        public IReadOnlyList<OperatorEntityNullableInt> OperatorEntitiesNullableInt { get; }
        public IReadOnlyList<OperatorEntityBool> OperatorEntitiesBool { get; }
        public IReadOnlyList<OperatorEntityNullableBool> OperatorEntitiesNullableBool { get; }
        public IReadOnlyList<OperatorEntityDateTimeOffset> OperatorEntitiesDateTimeOffset { get; }

        private OperatorsData()
        {
            OperatorEntitiesString = CreateStrings();
            OperatorEntitiesInt = CreateInts();
            OperatorEntitiesNullableInt = CreateNullableInts();
            OperatorEntitiesBool = CreateBools();
            OperatorEntitiesNullableBool = CreateNullableBools();
            OperatorEntitiesDateTimeOffset = CreateDateTimeOffsets();
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

        public static IReadOnlyList<OperatorEntityString> CreateStrings()
            => new List<OperatorEntityString>
            {
                new()
                {
                    Id = 1,
                    Value = "A",
                },
                new()
                {
                    Id = 2,
                    Value = "B",
                },
                new()
                {
                    Id = 3,
                    Value = "AB",
                },
                new()
                {
                    Id = 4,
                    Value = "BA",
                }
            };

        public static IReadOnlyList<OperatorEntityInt> CreateInts()
            => new List<OperatorEntityInt>
            {
                new()
                {
                    Id = 1,
                    Value = 1,
                },
                new()
                {
                    Id = 2,
                    Value = 2,
                },
                new()
                {
                    Id = 3,
                    Value = 3,
                },
                new()
                {
                    Id = 4,
                    Value = 5,
                },
                new()
                {
                    Id = 5,
                    Value = 8,
                },
            };

        public static IReadOnlyList<OperatorEntityNullableInt> CreateNullableInts()
            => new List<OperatorEntityNullableInt>
            {
                new()
                {
                    Id = 1,
                    Value = null,
                },
                new()
                {
                    Id = 2,
                    Value = 1,
                },
                new()
                {
                    Id = 3,
                    Value = 2,
                },
                new()
                {
                    Id = 4,
                    Value = 8,
                },
            };


        public static IReadOnlyList<OperatorEntityBool> CreateBools()
            => new List<OperatorEntityBool>
            {
                new()
                {
                    Id = 1,
                    Value = true,
                },
                new()
                {
                    Id = 2,
                    Value = false,
                },
            };

        public static IReadOnlyList<OperatorEntityNullableBool> CreateNullableBools()
            => new List<OperatorEntityNullableBool>
            {
                new()
                {
                    Id = 1,
                    Value = true,
                },
                new()
                {
                    Id = 2,
                    Value = false,
                },
                new()
                {
                    Id = 3,
                    Value = null,
                },
            };

        public static IReadOnlyList<OperatorEntityDateTimeOffset> CreateDateTimeOffsets()
            => new List<OperatorEntityDateTimeOffset>
            {
                new()
                {
                    Id = 1,
                    Value = new DateTimeOffset(new DateTime(2000, 1, 1, 11, 0, 0), new TimeSpan(5, 10, 0)),
                },
                new()
                {
                    Id = 2,
                    Value = new DateTimeOffset(new DateTime(2000, 1, 1, 10, 0, 0), new TimeSpan(-8, 0, 0)),
                },
                new()
                {
                    Id = 3,
                    Value = new DateTimeOffset(new DateTime(2000, 1, 1, 9, 0, 0), new TimeSpan(13, 0, 0)),
                }
            };
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

    public class OperatorDto2<TEntity1, TEntity2, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
    {
        public OperatorDto2(TEntity1 entity1, TEntity2 entity2, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }

        public TResult Result { get; set; }
    }

    public class OperatorDto3<TEntity1, TEntity2, TEntity3, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
    {
        public OperatorDto3(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Entity3 = entity3;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }
        public TEntity3 Entity3 { get; set; }

        public TResult Result { get; set; }
    }

    #endregion
}
