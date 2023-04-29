//
//  NetworkManagerOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="NetworkManager"/>.
/// </summary>
public class NetworkManagerOptions
{
    /// <summary>
    /// Gets or sets the pattern to find the network object at.
    /// </summary>
    /// <remarks>
    /// The address of the object is "three pointers down" from address found on this pattern.
    /// </remarks>
    public string NetworkObjectPattern { get; set; }
        = "A1 ?? ?? ?? ?? 8B 00 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? A1 ?? ?? ?? ?? 8B 00 8B 40 40";

    /// <summary>
    /// Gets or sets the offset of NetworkObject.
    /// </summary>
    public int NetworkObjectOffset { get; set; } = 1;
}