// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class representing a component of JSON path used in <see cref="JsonQueryExpression" /> or <see cref="JsonScalarExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class PathSegment
{
    /// <summary>
    ///     Creates a new instance of the <see cref="PathSegment" /> class which represents a property access.
    /// </summary>
    /// <param name="key">A key which is being accessed in the JSON.</param>
    public PathSegment(string key)
    {
        Key = key;
        CollectionIndexExpression = null;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="PathSegment" /> class which represents a collection element access.
    /// </summary>
    /// <param name="collectionIndexExpression">A collection index which is being accessed in the JSON.</param>
    public PathSegment(SqlExpression collectionIndexExpression)
    {
        CollectionIndexExpression = collectionIndexExpression;
        Key = null;
    }

    ///// <summary>
    /////     Creates a new instance of the <see cref="PathSegment" /> class.
    ///// </summary>
    ///// <param name="key">A key which is being accessed in the JSON.</param>
    ///// <param name="collectionIndexExpression">A collection index which is being accessed in the JSON.</param>
    //public PathSegment(string key, SqlExpression collectionIndexExpression)
    //    : this(key)
    //{
    //    CollectionIndexExpression = collectionIndexExpression;
    //}

    /// <summary>
    ///     The key which is being accessed in the JSON.
    /// </summary>
    public virtual string? Key { get; }

    /// <summary>
    ///     The index of the collection which is being accessed in the JSON.
    /// </summary>
    public virtual SqlExpression? CollectionIndexExpression { get; }

    /// <inheritdoc />
    public override string ToString()
        => (Key == "$" ? "" : ".")
        + (Key ?? "")
        + (CollectionIndexExpression == null ? "" : $"[{CollectionIndexExpression}]");

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is PathSegment pathSegment
                && Equals(pathSegment));

    private bool Equals(PathSegment pathSegment)
        => Key == pathSegment.Key
            && ((CollectionIndexExpression == null && pathSegment.CollectionIndexExpression == null)
                || (CollectionIndexExpression != null && CollectionIndexExpression.Equals(pathSegment.CollectionIndexExpression)));

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Key, CollectionIndexExpression);
}
