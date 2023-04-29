//
//  HookStateEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Extensions.SharedBinding.EventArgs;

/// <inheritdoc />
public class HookStateEventArgs : System.EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HookStateEventArgs"/> class.
    /// </summary>
    /// <param name="enabled">The new state.</param>
    public HookStateEventArgs(bool enabled)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Gets the new state.
    /// </summary>
    public bool Enabled { get; }
}