//
//  PacketEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace NosSmooth.LocalBinding.EventArgs;

/// <summary>
/// Event arguments for packet events.
/// </summary>
public class PacketEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets the packet string.
    /// </summary>
    public string Packet { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketEventArgs"/> class.
    /// </summary>
    /// <param name="packet">The packet.</param>
    public PacketEventArgs(string packet)
    {
        Packet = packet;
    }
}