//
//  NtClientOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Structs;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="NtClient"/>.
/// </summary>
public class NtClientOptions
{
    /// <summary>
    /// Gets or sets the pattern to find static pet manager list address at.
    /// </summary>
    public string NtClientPattern { get; set; }
        = "FF FF 00 00 00 00 FF FF FF FF ?? ?? ?? ?? 00 ?? 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? FF FF FF 00 00 00 00 00 00 00 00 00 00 00";

    /// <summary>
    /// Gets or sets the offsets to find the scene manager at from the static address.
    /// </summary>
    public int[] NtClientOffsets { get; set; }
        = { 10 };
}