// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class FilteredQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : class, IQueryFixtureBase, new()
{
    protected FilteredQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public Task AssertFilteredQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Func<TResult, object> elementSorter = null,
        Action<TResult, TResult> elementAsserter = null,
        bool assertOrder = false,
        int entryCount = 0,
        bool assertEmptyResult = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : class
        => AssertFilteredQuery(async, query, query, elementSorter, elementAsserter, assertOrder, entryCount, assertEmptyResult, testMethodName);

    public Task AssertFilteredQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<TResult, object> elementSorter = null,
        Action<TResult, TResult> elementAsserter = null,
        bool assertOrder = false,
        int entryCount = 0,
        bool assertEmptyResult = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : class
        => QueryAsserter.AssertQuery(
            actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount, assertEmptyResult, async, testMethodName,
            filteredQuery: true);

    public Task AssertFilteredQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        bool assertOrder = false,
        bool assertEmptyResult = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : struct
        => AssertFilteredQueryScalar(async, query, query, assertOrder, assertEmptyResult, testMethodName);

    public Task AssertFilteredQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        bool assertOrder = false,
        bool assertEmptyResult = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : struct
        => QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder, async, assertEmptyResult, testMethodName, filteredQuery: true);

    protected Task AssertFilteredCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query)
        => AssertFilteredCount(async, query, query);

    protected Task AssertFilteredCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery)
        => QueryAsserter.AssertCount(actualQuery, expectedQuery, async, filteredQuery: true);
}
