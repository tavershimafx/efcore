// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     <para>
///         Represents a factory for key values based on the primary/principal key values taken from various forms of entity data.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IPrincipalKeyValueFactory
{
    /// <summary>
    ///     Creates a key object from key values obtained in-order from the given array.
    /// </summary>
    /// <param name="keyValues">The key values.</param>
    /// <returns>The key object, or null if any of the key values were null.</returns>
    object? CreateFromKeyValues(IEnumerable<object?> keyValues);

    /// <summary>
    ///     Creates a key object from key values obtained from their indexed position in the given <see cref="ValueBuffer" />.
    /// </summary>
    /// <param name="valueBuffer">The buffer containing key values.</param>
    /// <returns>The key object, or null if any of the key values were null.</returns>
    object? CreateFromBuffer(ValueBuffer valueBuffer);

    /// <summary>
    ///     Finds the first null in the given in-order array of key values and returns the associated <see cref="IProperty" />.
    /// </summary>
    /// <param name="keyValues">The key values.</param>
    /// <returns>The associated property.</returns>
    IProperty? FindNullPropertyInKeyValues(object?[] keyValues);

    /// <summary>
    ///     Creates a key object from the key values in the given entry.
    ///     Creates an equatable key object from the key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <param name="fromOriginalValues">Whether the original or current value should be used.</param>
    /// <returns>The key value.</returns>
    object CreateEquatableKey(IUpdateEntry entry, bool fromOriginalValues = false);
}
