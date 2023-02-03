// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OperatorsQuerySqliteTest : OperatorsQueryTestBase
{
    public OperatorsQuerySqliteTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override void Seed(OperatorsContext ctx)
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
}
