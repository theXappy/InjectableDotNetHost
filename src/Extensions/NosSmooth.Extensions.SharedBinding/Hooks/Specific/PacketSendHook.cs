//
//  PacketSendHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <inheritdoc />
internal class PacketSendHook : SingleHook<IPacketSendHook.PacketSendDelegate,
    IPacketSendHook.PacketSendWrapperDelegate, PacketEventArgs>, IPacketSendHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacketSendHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public PacketSendHook(INostaleHook<IPacketSendHook.PacketSendDelegate, IPacketSendHook.PacketSendWrapperDelegate, PacketEventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}