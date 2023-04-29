//
//  IPacketReceiveHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Objects;
using Reloaded.Hooks.Definitions.X86;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of NetworkManager.PacketSend.
/// </summary>
public interface IPacketReceiveHook : INostaleHook<IPacketReceiveHook.PacketReceiveDelegate,
    IPacketReceiveHook.PacketReceiveWrapperDelegate, PacketEventArgs>
{
    /// <summary>
    /// NosTale packet send function to hook.
    /// </summary>
    /// <param name="packetObject">The packet object.</param>
    /// <param name="packetString">Pointer to <see cref="NostaleStringA"/>.Get() object.</param>
    /// <returns>1 to proceed to NosTale function, 0 to block the call.</returns>
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    public delegate nuint PacketReceiveDelegate(nuint packetObject, nuint packetString);

    /// <summary>
    /// Packet send function.
    /// </summary>
    /// <param name="packetString">The string to send.</param>
    public delegate void PacketReceiveWrapperDelegate(string packetString);
}