//
//  WalkEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace NosSmooth.LocalBinding.EventArgs;

/// <summary>
/// Event args for walk functions.
/// </summary>
public class WalkEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets the x coordinate to walk to.
    /// </summary>
    public ushort X { get; }

    /// <summary>
    /// Gets the y coordinate to walk to.
    /// </summary>
    public ushort Y { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkEventArgs"/> class.
    /// </summary>
    /// <param name="x">The x coordinate to walk to.</param>
    /// <param name="y">The y coordinate to walk to.</param>
    public WalkEventArgs(ushort x, ushort y)
    {
        X = x;
        Y = y;
    }
}