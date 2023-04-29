//
//  PacketReceiveHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <inheritdoc />
internal class PacketReceiveHook : SingleHook<IPacketReceiveHook.PacketReceiveDelegate,
    IPacketReceiveHook.PacketReceiveWrapperDelegate, PacketEventArgs>, IPacketReceiveHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacketReceiveHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public PacketReceiveHook(INostaleHook<IPacketReceiveHook.PacketReceiveDelegate, IPacketReceiveHook.PacketReceiveWrapperDelegate, PacketEventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}