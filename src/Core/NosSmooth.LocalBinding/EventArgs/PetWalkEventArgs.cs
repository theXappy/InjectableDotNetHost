//
//  PetWalkEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Structs;

namespace NosSmooth.LocalBinding.EventArgs;

/// <summary>
/// Event args for pet walking.
/// </summary>
public class PetWalkEventArgs : WalkEventArgs
{
    /// <summary>
    /// Gets the pet manager for the pet that is moving.
    /// </summary>
    public PetManager PetManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PetWalkEventArgs"/> class.
    /// </summary>
    /// <param name="petManager">The pet manager of the walking pet.</param>
    /// <param name="x">The x coordinate to walk to.</param>
    /// <param name="y">The y coordinate to walk to.</param>
    public PetWalkEventArgs(PetManager petManager, ushort x, ushort y)
        : base(x, y)
    {
        PetManager = petManager;
    }
}