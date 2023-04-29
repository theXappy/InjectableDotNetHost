//
//  NetworkManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Extensions;
using NosSmooth.LocalBinding.Options;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding.Structs;

/// <summary>
/// An object used for PacketSend and PacketReceive functions.
/// </summary>
public class NetworkManager : NostaleObject
{
    /// <summary>
    /// Create <see cref="PlayerManager"/> instance.
    /// </summary>
    /// <param name="nosBrowserManager">The NosTale process browser.</param>
    /// <param name="options">The options.</param>
    /// <returns>The player manager or an error.</returns>
    public static Result<NetworkManager> Create(NosBrowserManager nosBrowserManager, NetworkManagerOptions options)
    {
        var networkObjectAddress = nosBrowserManager.Scanner.FindPattern(options.NetworkObjectPattern);
        if (!networkObjectAddress.Found)
        {
            return new BindingNotFoundError(options.NetworkObjectPattern, "NetworkBinding");
        }

        if (nosBrowserManager.Process.MainModule is null)
        {
            return new NotFoundError("Cannot find the main module of the target process.");
        }

        var staticAddress = (nuint)(nosBrowserManager.Process.MainModule.BaseAddress + networkObjectAddress.Offset
            + options.NetworkObjectOffset);
        return new NetworkManager(nosBrowserManager.Memory, staticAddress);
    }

    private readonly IMemory _memory;
    private readonly nuint _staticNetworkManagerAddress;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkManager"/> class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="staticNetworkManagerAddress">The pointer to the beginning of the player manager structure.</param>
    public NetworkManager(IMemory memory, nuint staticNetworkManagerAddress)
        : base(memory, staticNetworkManagerAddress)
    {
        _memory = memory;
        _staticNetworkManagerAddress = staticNetworkManagerAddress;
    }

    /// <summary>
    /// Gets the address to the player manager.
    /// </summary>
    public override nuint Address => GetManagerAddress(false);

    private nuint GetManagerAddress(bool third)
    {
        nuint networkManager = _staticNetworkManagerAddress;
        _memory.Read(networkManager, out networkManager);
        _memory.Read(networkManager, out networkManager);
        _memory.Read(networkManager, out networkManager);

        if (third)
        {
            _memory.Read(networkManager + 0x34, out networkManager);
        }

        return networkManager;
    }

    /// <summary>
    /// Gets an address pointer used in PacketSend function.
    /// </summary>
    /// <returns>Pointer to the object.</returns>
    public nuint GetAddressForPacketSend()
        => GetManagerAddress(false);

    /// <summary>
    /// Gets an address pointer used in PacketReceive function.
    /// </summary>
    /// <returns>Pointer to the object.</returns>
    public nuint GetAddressForPacketReceive()
        => GetManagerAddress(true);
}