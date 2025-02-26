﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OptionalDependentQuerySqlServerTest : OptionalDependentQueryTestBase<OptionalDependentQuerySqlServerFixture>
{
    public OptionalDependentQuerySqlServerTest(OptionalDependentQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
    public override async Task Basic_projection_entity_with_all_optional(bool async)
    {
        await base.Basic_projection_entity_with_all_optional(async);

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM [EntitiesAllOptional] AS [e]
""");
    }

    public override async Task Basic_projection_entity_with_some_required(bool async)
    {
        await base.Basic_projection_entity_with_some_required(async);

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM [EntitiesSomeRequired] AS [e]
""");
    }

    public override async Task Filter_optional_dependent_with_all_optional_compared_to_null(bool async)
    {
        await base.Filter_optional_dependent_with_all_optional_compared_to_null(async);

        AssertSql("");
    }

    public override async Task Filter_optional_dependent_with_all_optional_compared_to_not_null(bool async)
    {
        await base.Filter_optional_dependent_with_all_optional_compared_to_not_null(async);

        AssertSql("");
    }

    public override async Task Filter_optional_dependent_with_some_required_compared_to_null(bool async)
    {
        await base.Filter_optional_dependent_with_some_required_compared_to_null(async);

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM [EntitiesSomeRequired] AS [e]
WHERE CAST(JSON_VALUE([e].[Json], '$.ReqProp') AS float) IS NULL
""");
    }

    public override async Task Filter_optional_dependent_with_some_required_compared_to_not_null(bool async)
    {
        await base.Filter_optional_dependent_with_some_required_compared_to_not_null(async);

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM [EntitiesSomeRequired] AS [e]
WHERE CAST(JSON_VALUE([e].[Json], '$.ReqProp') AS float) IS NOT NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_all_optional_compared_to_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_all_optional_compared_to_null(async);

        AssertSql("");
    }

    public override async Task Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_all_optional_compared_to_not_null(async);

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM [EntitiesAllOptional] AS [e]
WHERE CAST(JSON_VALUE([e].[Json], '$.OpNested2.ReqNested1') AS bit) IS NOT NULL AND CAST(JSON_VALUE([e].[Json], '$.OpNested2.ReqNested2') AS datetime2) IS NOT NULL
""");
    }

    public override async Task Filter_nested_optional_dependent_with_some_required_compared_to_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_some_required_compared_to_null(async);

        AssertSql("");
    }

    public override async Task Filter_nested_optional_dependent_with_some_required_compared_to_not_null(bool async)
    {
        await base.Filter_nested_optional_dependent_with_some_required_compared_to_not_null(async);

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], [e].[Json]
FROM [EntitiesSomeRequired] AS [e]
WHERE CAST(JSON_VALUE([e].[Json], '$.ReqNested2.ReqNested1') AS bit) IS NOT NULL AND CAST(JSON_VALUE([e].[Json], '$.ReqNested2.ReqNested2') AS datetime2) IS NOT NULL
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
