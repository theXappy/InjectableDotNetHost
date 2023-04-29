//
//  UnitManagerOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="UnitManager"/>.
/// </summary>
public class UnitManagerOptions
{
    /// <summary>
    /// Gets or sets the pattern to static address of unit manager.
    /// </summary>
    public string UnitManagerPattern { get; set; }
        = "A1 ?? ?? ?? ?? E8 ?? ?? ?? ?? 33 C0 5A 59 59 64 89 10 68 ?? ?? ?? ?? 8D 45 F0 BA";

    /// <summary>
    /// Gets or sets the pointer offsets from the unit manager static address.
    /// </summary>
    public int[] UnitManagerOffsets { get; set; }
        = { 1, 0 };
}