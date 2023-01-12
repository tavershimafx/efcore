// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OperatorsQueryTestBase : NonSharedModelTestBase
{
    protected readonly List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> Binaries;
    protected readonly List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> Unaries;
    protected readonly Dictionary<Type, Type> PropertyTypeToEntityMap;

    protected OperatorsData ExpectedData { get; init; }
    protected ExpectedQueryRewritingVisitor ExpectedQueryRewriter { get; init; }

    protected OperatorsQueryTestBase(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);

        Binaries = new()
        {
            ((typeof(int), typeof(int)), typeof(int), Expression.Multiply),
            ((typeof(int), typeof(int)), typeof(int), Expression.Divide),
            ((typeof(int), typeof(int)), typeof(int), Expression.Modulo),
            ((typeof(int), typeof(int)), typeof(int), Expression.Add),
            ((typeof(int), typeof(int)), typeof(int), Expression.Subtract),

            ((typeof(bool), typeof(bool)), typeof(bool), Expression.And),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Or),

            //((typeof(int), typeof(int)), typeof(int), Expression.LeftShift),
            //((typeof(int), typeof(int)), typeof(int), Expression.RightShift),

            ((typeof(int), typeof(int)), typeof(bool), Expression.LessThan),
            ((typeof(int), typeof(int)), typeof(bool), Expression.LessThanOrEqual),
            ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThan),
            ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThanOrEqual),

            ((typeof(int), typeof(int)), typeof(bool), Expression.Equal),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Equal),
            ((typeof(string), typeof(string)), typeof(bool), Expression.Equal),
            //((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.Equal),

            ((typeof(int), typeof(int)), typeof(bool), Expression.NotEqual),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.NotEqual),
            ((typeof(string), typeof(string)), typeof(bool), Expression.NotEqual),
            //((typeof(DateTimeOffset), typeof(DateTimeOffset)), typeof(bool), Expression.NotEqual),

            ((typeof(bool), typeof(bool)), typeof(bool), Expression.AndAlso),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.OrElse),
        };

        Unaries = new()
        {
            (typeof(bool), typeof(bool), Expression.Not),
            (typeof(int), typeof(int), Expression.Not),
            (typeof(int), typeof(int), Expression.Negate),

            (typeof(bool?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(bool?)))),
            (typeof(int?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(int?)))),
            (typeof(string), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(string)))),

            (typeof(bool?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(bool?)))),
            (typeof(int?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(int?)))),
            (typeof(string), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(string)))),
        };

        PropertyTypeToEntityMap = new()
        {
            { typeof(string), typeof(OperatorEntityString) },
            { typeof(int), typeof(OperatorEntityInt) },
            { typeof(int?), typeof(OperatorEntityNullableInt) },
            { typeof(bool), typeof(OperatorEntityBool) },
            { typeof(bool?), typeof(OperatorEntityNullableBool) },
        };

        ExpectedData = OperatorsData.Instance;
        ExpectedQueryRewriter = new ExpectedQueryRewritingVisitor(Expression.Constant(ExpectedData));
    }

    protected override string StoreName
        => "OperatorsTest";

    [ConditionalFact]
    public virtual async Task Basic_binary_in_projection()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            foreach (var binary in Binaries.Where(x => x.ResultType != typeof(bool)))
            {
                TestProjectionQueryWithTwoSources(
                    binary.InputTypes.Item1,
                    binary.InputTypes.Item2,
                    binary.ResultType,
                    context,
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
            foreach (var binary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                TestPredicateQueryWithTwoSources(
                    binary.InputTypes.Item1,
                    binary.InputTypes.Item2,
                    context,
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
            foreach (var unary in Unaries.Where(x => x.InputType != typeof(bool)))
            {
                foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
                {
                    var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

                    TestProjectionQueryWithTwoSources(
                        binary.InputTypes.Item1,
                        binary.InputTypes.Item2,
                        binary.ResultType,
                        context,
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
            foreach (var unary in Unaries.Where(x => x.InputType == typeof(bool)))
            {
                foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
                {
                    var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

                    TestPredicateQueryWithTwoSources(
                        binary.InputTypes.Item1,
                        binary.InputTypes.Item2,
                        context,
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
            foreach (var binary in Binaries.Where(x => x.ResultType != typeof(bool)))
            {
                foreach (var unary1 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item1))
                {
                    foreach (var unary2 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item2))
                    {
                        var operatorCreator = (Expression l, Expression r) => binary.OperatorCreator(unary1.OperatorCreator(l), unary2.OperatorCreator(r));

                        TestProjectionQueryWithTwoSources(
                            unary1.InputType,
                            unary2.InputType,
                            binary.ResultType,
                            context,
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
            foreach (var binary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var unary1 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item1))
                {
                    foreach (var unary2 in Unaries.Where(x => x.ResultType == binary.InputTypes.Item2))
                    {
                        var operatorCreator = (Expression l, Expression r) => binary.OperatorCreator(unary1.OperatorCreator(l), unary2.OperatorCreator(r));

                        TestPredicateQueryWithTwoSources(
                            unary1.InputType,
                            unary2.InputType,
                            context,
                            operatorCreator);
                    }
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
            foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item1))
                {
                    var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(innerBinary.OperatorCreator(f, s), t);

                    TestPredicateQueryWithThreeSources(
                        innerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item2,
                        outerBinary.InputTypes.Item2,
                        context,
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
            foreach (var outerBinary in Binaries.Where(x => x.ResultType == typeof(bool)))
            {
                foreach (var innerBinary in Binaries.Where(x => x.ResultType == outerBinary.InputTypes.Item2))
                {
                    var operatorCreator = (Expression f, Expression s, Expression t) => outerBinary.OperatorCreator(f, innerBinary.OperatorCreator(s, t));

                    TestPredicateQueryWithThreeSources(
                        outerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item1,
                        innerBinary.InputTypes.Item2,
                        context,
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
            foreach (var unary in Unaries.Where(x => x.InputType != typeof(bool)))
            {
                foreach (var binary in Binaries.Where(x => x.ResultType == unary.InputType))
                {
                    var operatorCreator = (Expression l, Expression r) => unary.OperatorCreator(binary.OperatorCreator(l, r));

                    TestProjectionQueryWithTwoSources(
                        binary.InputTypes.Item1,
                        binary.InputTypes.Item2,
                        binary.ResultType,
                        context,
                        operatorCreator);
                }
            }
        }
    }

    protected class ExpectedQueryRewritingVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _contextSetMethod = typeof(DbContext).GetRuntimeMethod(nameof(DbContext.Set), new Type[] { });
        private readonly MethodInfo _expectedDataSetMethod = typeof(ISetSource).GetRuntimeMethod(nameof(DbContext.Set), new Type[] { });

        private readonly ConstantExpression _expectedDataExpression;

        public ExpectedQueryRewritingVisitor(ConstantExpression expectedDataExpression)
        {
            _expectedDataExpression = expectedDataExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method is MethodInfo { IsGenericMethod: true } method
                && method.GetGenericMethodDefinition() == _contextSetMethod)
            {
                var typeArgument = method.GetGenericArguments().Single();

                return Expression.Call(
                    _expectedDataExpression,
                    _expectedDataSetMethod.MakeGenericMethod(typeArgument));
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is QueryRootExpression queryRoot)
            {
                return Expression.Call(
                    _expectedDataExpression,
                    _expectedDataSetMethod.MakeGenericMethod(queryRoot.ElementType));
            }

            return base.VisitExtension(extensionExpression);
        }
    }


    #region projection

    private void TestProjectionQueryWithTwoSources(
        Type firstType,
        Type secondType,
        Type resultType,
        OperatorsContext context,
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
                context,
                resultCreator
            });
    }

    private void TestProjectionQueryWithThreeSources(
        Type firstType,
        Type secondType,
        Type thirdType,
        Type resultType,
        OperatorsContext context,
        Func<Expression, Expression, Expression> resultCreator)
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
            null,
            new object[]
            {
                context,
                resultCreator
            });
    }

    private class DefaultSetSource : ISetSource
    {
        private readonly DbContext _context;

        public DefaultSetSource(DbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();
    }

    private void TestProjectionQueryWithTwoSourcesInternal<TFirst, TSecond, TResult>(
        OperatorsContext context,
        Func<Expression, Expression, Expression> resultCreator)
        where TFirst : class
        where TSecond : class
    {
        var setSource = new DefaultSetSource(context);
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TFirst>()
            from e2 in ss.Set<TSecond>()
            select new OperatorDto2<TFirst, TSecond, TResult>(e1, e2, default);

        var actualQueryTemplate = setSourceTemplate(setSource);
        var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(actualRewritten);
        var actualResults = actualQuery.ToList();

        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(expectedRewritten);
        var expectedResults = expectedQuery.ToList();









        //var queryTemplate =
        //    from e1 in context.Set<TFirst>()
        //    from e2 in context.Set<TSecond>()
        //    select new OperatorDto2<TFirst, TSecond, TResult>(e1, e2, default);

        //var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
        //var rewritten = resultRewriter.Visit(queryTemplate.Expression);
        //var query = queryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(rewritten);

        //var result = query.ToList();

        //var expected = ExpectedQueryRewriter.Visit(rewritten);



        //var blah = queryTemplate.Provider.CreateQuery<OperatorDto2<TFirst, TSecond, TResult>>(expected);




        //var fybr = blah.ToList();
    }

    private void TestProjectionQueryWithThreeSourcesInternal<TFirst, TSecond, TThird, TResult>(
        OperatorsContext context,
        Func<Expression, Expression, Expression, Expression> resultCreator)
        where TFirst : class
        where TSecond : class
        where TThird : class
    {
        var queryTemplate =
            from e1 in context.Set<TFirst>()
            from e2 in context.Set<TSecond>()
            from e3 in context.Set<TThird>()
            select new OperatorDto3<TFirst, TSecond, TThird, TResult>(e1, e2, e3, default);

        var resultRewriter = new ResultExpressionProjectionRewriter(resultCreator);
        var rewritten = resultRewriter.Visit(queryTemplate.Expression);
        var query = queryTemplate.Provider.CreateQuery<OperatorDto3<TFirst, TSecond, TThird, TResult>>(rewritten);

        var result = query.ToList();
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
        Type firstType,
        Type secondType,
        OperatorsContext context,
        Func<Expression, Expression, Expression> resultCreator)
    {
        var method = typeof(OperatorsQueryTestBase).GetMethod(
            nameof(TestPredicateQueryWithTwoSourcesInternal),
            BindingFlags.NonPublic | BindingFlags.Static);

        var genericMethod = method.MakeGenericMethod(
            PropertyTypeToEntityMap[firstType],
            PropertyTypeToEntityMap[secondType]);

        genericMethod.Invoke(
            null,
            new object[]
            {
                context,
                resultCreator
            });
    }

    private void TestPredicateQueryWithThreeSources(
        Type firstType,
        Type secondType,
        Type thirdType,
        OperatorsContext context,
        Func<Expression, Expression, Expression, Expression> resultCreator)
    {
        var method = typeof(OperatorsQueryTestBase).GetMethod(
            nameof(TestPredicateQueryWithThreeSourcesInternal),
            BindingFlags.NonPublic | BindingFlags.Static);

        var genericMethod = method.MakeGenericMethod(
            PropertyTypeToEntityMap[firstType],
            PropertyTypeToEntityMap[secondType],
            PropertyTypeToEntityMap[thirdType]);

        genericMethod.Invoke(
            null,
            new object[]
            {
                context,
                resultCreator
            });
    }

    private static void TestPredicateQueryWithTwoSourcesInternal<TFirst, TSecond>(
        OperatorsContext context,
        Func<Expression, Expression, Expression> resultCreator)
        where TFirst : class
        where TSecond : class
    {
        var queryTemplate =
            from e1 in context.Set<TFirst>()
            from e2 in context.Set<TSecond>()
            where DummyTrue(e1, e2)
            select new ValueTuple<TFirst, TSecond>(e1, e2);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultCreator);
        var rewritten = resultRewriter.Visit(queryTemplate.Expression);
        var query = queryTemplate.Provider.CreateQuery<ValueTuple<TFirst, TSecond>>(rewritten);

        var result = query.ToList();
    }

    private static void TestPredicateQueryWithThreeSourcesInternal<TFirst, TSecond, TThird>(
        OperatorsContext context,
        Func<Expression, Expression, Expression, Expression> resultCreator)
        where TFirst : class
        where TSecond : class
        where TThird : class
    {
        var queryTemplate =
            from e1 in context.Set<TFirst>()
            from e2 in context.Set<TSecond>()
            from e3 in context.Set<TThird>()
            where DummyTrue(e1, e2, e3)
            select new ValueTuple<TFirst, TSecond, TThird>(e1, e2, e3);

        var resultRewriter = new ResultExpressionPredicateRewriter(resultCreator);
        var rewritten = resultRewriter.Visit(queryTemplate.Expression);
        var query = queryTemplate.Provider.CreateQuery<ValueTuple<TFirst, TSecond, TThird>>(rewritten);

        var result = query.ToList();
    }

    private static bool DummyTrue<TFirst, TSecond>(TFirst first, TSecond second)
        => true;

    private static bool DummyTrue<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third)
        => true;

    private class ResultExpressionPredicateRewriter : ExpressionVisitor
    {
        private readonly Func<Expression, Expression, Expression> _resultCreatorTwoArgs;
        private readonly Func<Expression, Expression, Expression, Expression> _resultCreatorThreeArgs;

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

    protected abstract void Seed(OperatorsContext ctx);

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
        }
    }

    protected class OperatorsData : ISetSource
    {
        public static readonly OperatorsData Instance = new();

        public IReadOnlyList<OperatorEntityString> OperatorEntitiesString { get; }
        public IReadOnlyList<OperatorEntityInt> OperatorEntitiesInt { get; }
        public IReadOnlyList<OperatorEntityBool> OperatorEntitiesBool { get; }

        private OperatorsData()
        {
            OperatorEntitiesString = CreateStrings();
            OperatorEntitiesInt = CreateInts();
            OperatorEntitiesBool = CreateBools();
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

            if (typeof(TEntity) == typeof(OperatorEntityBool))
            {
                return (IQueryable<TEntity>)OperatorEntitiesBool.AsQueryable();
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
                    Value = "ABA",
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
    }

    public class OperatorEntityString
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class OperatorEntityInt
    {
        public int Id { get; set; }
        public int Value { get; set; }
    }

    public class OperatorEntityNullableInt
    {
        public int Id { get; set; }
        public int? Value { get; set; }
    }

    public class OperatorEntityBool
    {
        public int Id { get; set; }
        public bool Value { get; set; }
    }

    public class OperatorEntityNullableBool
    {
        public int Id { get; set; }
        public bool? Value { get; set; }
    }

    public class OperatorDto2<TEntity1, TEntity2, TResult>
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
}
