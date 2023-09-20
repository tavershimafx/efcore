// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonOwnedOptionalDependentAllOptional
{
    public int? Number { get; set; }
    public DateTime? Dob { get; set; }

    public JsonOwnedOptionalDependentNested NestedAllOptional { get; set; }
}
