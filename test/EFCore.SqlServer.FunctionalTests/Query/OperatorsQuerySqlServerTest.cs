// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OperatorsQuerySqlServerTest : OperatorsQueryTestBase
{
    public OperatorsQuerySqlServerTest(ITestOutputHelper testOutputHelper)
    : base(testOutputHelper)
    {
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override void Seed(OperatorsContext ctx)
    {
        var strings = new List<string> { null, "A", "AB" };
        var numbers = new List<int?> { null, 2, 4, };
        var bools = new List<bool?> { null, true, false };
        var datetimes = new List<DateTime?> { null, new DateTime(2000, 1, 1), new DateTime(2022, 10, 10) };

        foreach (var s in strings)
        {
            foreach (var n in numbers)
            {
                foreach (var b in bools)
                {
                    foreach (var d in datetimes)
                    {
                        var entity = new OperatosEntity
                        {
                            String = s,
                            Number = n,
                            Bool = b,
                            DateTime = d,
                        };


                        ctx.Entities.Add(entity);
                    }
                }
            }
        }

        ctx.SaveChanges();
    }
}
