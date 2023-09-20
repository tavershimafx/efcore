// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonOwnedOptionalDependentSomeRequired
{
    public int Foo { get; set; }

    public decimal Bar { get; set; }

    public int? Number { get; set; }
    public DateTime? Dob { get; set; }

    public JsonOwnedOptionalDependentNested Nested { get; set; }
}
