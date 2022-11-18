// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OperatorsQueryTestBase : NonSharedModelTestBase
{
    private readonly List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> _binaries = new()
    {
        ((typeof(int), typeof(int)), typeof(int), Expression.Multiply),
        ((typeof(int), typeof(int)), typeof(int), Expression.Divide),
        ((typeof(int), typeof(int)), typeof(int), Expression.Modulo),
        ((typeof(int), typeof(int)), typeof(int), Expression.Add),
        ((typeof(int), typeof(int)), typeof(int), Expression.Subtract),

        ((typeof(bool), typeof(bool)), typeof(bool), Expression.And),
        ((typeof(bool), typeof(bool)), typeof(bool), Expression.Or),

        ((typeof(int), typeof(int)), typeof(int), Expression.LeftShift),

        ((typeof(int), typeof(int)), typeof(int), Expression.RightShift),

        ((typeof(int), typeof(int)), typeof(bool), Expression.LessThan),
        ((typeof(int), typeof(int)), typeof(bool), Expression.LessThanOrEqual),
        ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThan),
        ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThanOrEqual),

        ((typeof(int), typeof(int)), typeof(bool), Expression.Equal),
        ((typeof(bool), typeof(bool)), typeof(bool), Expression.Equal),
        ((typeof(string), typeof(string)), typeof(bool), Expression.Equal),
        ((typeof(DateTime), typeof(DateTime)), typeof(bool), Expression.Equal),

        ((typeof(int), typeof(int)), typeof(bool), Expression.NotEqual),
        ((typeof(bool), typeof(bool)), typeof(bool), Expression.NotEqual),
        ((typeof(string), typeof(string)), typeof(bool), Expression.NotEqual),
        ((typeof(DateTime), typeof(DateTime)), typeof(bool), Expression.NotEqual),

        ((typeof(bool), typeof(bool)), typeof(bool), Expression.AndAlso),
        ((typeof(bool), typeof(bool)), typeof(bool), Expression.OrElse),
    };

    private readonly List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> _unaries = new()
    {
        (typeof(bool), typeof(bool), Expression.Not),
        (typeof(int), typeof(int), Expression.Not),

        (typeof(int), typeof(int), Expression.Negate),

        (typeof(int), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(int?)))),
        (typeof(string), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(string)))),
        (typeof(bool), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(bool?)))),
        (typeof(DateTime), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(DateTime?)))),

        (typeof(int), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(int?)))),
        (typeof(string), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(string)))),
        (typeof(bool), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(bool?)))),
        (typeof(DateTime), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(DateTime?)))),
    };

    protected OperatorsQueryTestBase(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override string StoreName
        => "OperatorsTest";








    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task BasicBinary(bool async)
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            foreach (var binary in _binaries.Where(x => x.ResultType == typeof(bool)))
            {
                var baseQuery =
                    from e1 in context.Entities
                    from e2 in context.Entities
                    select new { Entity1 = e1, Entity2 = e2 };

                var query = InjectOperatorsPredicateBinary(
                    baseQuery,
                    binary.InputTypes.Item1,
                    binary.InputTypes.Item2,
                    binary.OperatorCreator,
                    filterNulls: true);

                var result = async
                    ? await query.ToListAsync()
                    : query.ToList();
            }
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Binary_then_unary(bool async)
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            foreach (var binary in _binaries)
            {
                foreach (var unary in _unaries.Where(x => x.InputType == binary.ResultType))
                {
                    var baseQuery =
                        from e1 in context.Entities
                        from e2 in context.Entities
                        select new { Entity1 = e1, Entity2 = e2 };

                    var query = InjectOperatorsPredicateBinaryUnary(
                        baseQuery,
                        binary.InputTypes.Item1,
                        binary.InputTypes.Item2,
                        binary.OperatorCreator,
                        unary.OperatorCreator,
                        filterNulls: true);

                    var result = async
                        ? await query.ToListAsync()
                        : query.ToList();
                }
            }
        }
    }

    private IQueryable<T> InjectOperatorsPredicateBinary<T>(
        IQueryable<T> baseQuery,
        Type leftType,
        Type rightType,
        Func<Expression, Expression, Expression> operatorCreator,
        bool filterNulls)
    {
        //var topLevelSelect = baseQuery.Expression;
        //var selectMany = ((MethodCallExpression)topLevelSelect).Arguments[0];

        var selectMany = baseQuery.Expression;

        var predicateLambdaParameter = Expression.Parameter(typeof(T));
        var entityAccessor1 = Expression.Property(predicateLambdaParameter, "Entity1");
        var entityAccessor2 = Expression.Property(predicateLambdaParameter, "Entity2");

        var leftPropertyAccess = GetPropertyAccessCreator(leftType)(entityAccessor1);
        var rightPropertyAccess = GetPropertyAccessCreator(rightType)(entityAccessor2);


        Expression nullCheck = Expression.Constant(true);
        if (filterNulls)
        {
            nullCheck = AddNullCheck(nullCheck, leftPropertyAccess);
            nullCheck = AddNullCheck(nullCheck, rightPropertyAccess);

            if (leftPropertyAccess.Type.IsNullableType() && leftPropertyAccess.Type != typeof(string))
            {
                leftPropertyAccess = Expression.Property(leftPropertyAccess, "Value");
            }

            if (rightPropertyAccess.Type.IsNullableType() && rightPropertyAccess.Type != typeof(string))
            {
                rightPropertyAccess = Expression.Property(rightPropertyAccess, "Value");
            }
        }

        var operatorExpression = Expression.AndAlso(
            nullCheck,
            operatorCreator(leftPropertyAccess, rightPropertyAccess));

        var predicateLambda = Expression.Lambda<Func<T, bool>>(operatorExpression, predicateLambdaParameter);
        var whereMethod = QueryableMethods.Where.MakeGenericMethod(typeof(T));
        var whereMethodCall = Expression.Call(whereMethod, selectMany, predicateLambda);


        var result = baseQuery.Provider.CreateQuery<T>(whereMethodCall);

        return result;
    }

    private IQueryable<T> InjectOperatorsPredicateBinaryUnary<T>(
        IQueryable<T> baseQuery,
        Type leftType,
        Type rightType,
        Func<Expression, Expression, Expression> binaryOperatorCreator,
        Func<Expression, Expression> unaryOperatorCreator,
        bool filterNulls)
    {
        //var topLevelSelect = baseQuery.Expression;
        //var selectMany = ((MethodCallExpression)topLevelSelect).Arguments[0];

        var selectMany = baseQuery.Expression;

        var predicateLambdaParameter = Expression.Parameter(typeof(T));
        var entityAccessor1 = Expression.Property(predicateLambdaParameter, "Entity1");
        var entityAccessor2 = Expression.Property(predicateLambdaParameter, "Entity2");

        var leftPropertyAccess = GetPropertyAccessCreator(leftType)(entityAccessor1);
        var rightPropertyAccess = GetPropertyAccessCreator(rightType)(entityAccessor2);


        Expression nullCheck = Expression.Constant(true);
        if (filterNulls)
        {
            nullCheck = AddNullCheck(nullCheck, leftPropertyAccess);
            nullCheck = AddNullCheck(nullCheck, rightPropertyAccess);

            if (leftPropertyAccess.Type.IsNullableType() && leftPropertyAccess.Type != typeof(string))
            {
                leftPropertyAccess = Expression.Property(leftPropertyAccess, "Value");
            }

            if (rightPropertyAccess.Type.IsNullableType() && rightPropertyAccess.Type != typeof(string))
            {
                rightPropertyAccess = Expression.Property(rightPropertyAccess, "Value");
            }
        }

        var operatorExpression = Expression.AndAlso(
            nullCheck,
            unaryOperatorCreator(
                binaryOperatorCreator(leftPropertyAccess, rightPropertyAccess)));

        var predicateLambda = Expression.Lambda<Func<T, bool>>(operatorExpression, predicateLambdaParameter);
        var whereMethod = QueryableMethods.Where.MakeGenericMethod(typeof(T));
        var whereMethodCall = Expression.Call(whereMethod, selectMany, predicateLambda);


        var result = baseQuery.Provider.CreateQuery<T>(whereMethodCall);

        return result;
    }



    private Func<Expression, Expression> GetPropertyAccessCreator(Type propertyType)
        => propertyType switch
        {
            var type when type == typeof(int) => e => Expression.Property(e, "Number"),
            var type when type == typeof(string) => e => Expression.Property(e, "String"),
            var type when type == typeof(bool) => e => Expression.Property(e, "Bool"),
            var type when type == typeof(DateTime) => e => Expression.Property(e, "DateTime"),
            _ => throw new InvalidOperationException("Unhandled argument type")
        };

    private Expression AddNullCheck(Expression currentNullCheck, Expression expression)
        => expression.Type.IsNullableType()
            ? Expression.AndAlso(
                currentNullCheck,
                Expression.NotEqual(
                    expression,
                    Expression.Constant(null, expression.Type)))
        : currentNullCheck;

    protected abstract void Seed(OperatorsContext ctx);

    protected class OperatorsContext : DbContext
    {
        public OperatorsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<OperatosEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperatorEntityString>();
            modelBuilder.Entity<OperatorEntityInt>();
            modelBuilder.Entity<OperatorEntityNullableInt>();
            modelBuilder.Entity<OperatorEntityBool>();
            modelBuilder.Entity<OperatorEntityNullableBool>();
            modelBuilder.Entity<OperatorEntityDateTimeOffset>();
            modelBuilder.Entity<OperatorEntityNullableDateTimeOffset>();

            //modelBuilder.Entity<OperatosEntity1>().Property(x => x.Id).ValueGeneratedNever();
            //modelBuilder.Entity<OperatosEntity2>().Property(x => x.Id).ValueGeneratedNever();
            //modelBuilder.Entity<OperatosEntity3>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    public class OperatosEntity
    {
        public int Id { get; set; }
        public string String { get; set; }
        //public string String2 { get; set; }
        //public string String3 { get; set; }

        public int? Number { get; set; }
        //public int? Number2 { get; set; }
        //public int? Number3 { get; set; }

        public bool? Bool { get; set; }
        //public bool? Bool2 { get; set; }
        //public bool? Bool3 { get; set; }

        public DateTime? DateTime { get; set; }
        //public DateTime? DateTime2 { get; set; }
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

    public class OperatorEntityDateTimeOffset
    {
        public int Id { get; set; }
        public DateTimeOffset Value { get; set; }

    }

    public class OperatorEntityNullableDateTimeOffset
    {
        public int Id { get; set; }
        public DateTimeOffset? Value { get; set; }
    }
}
