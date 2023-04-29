//
//  NtClient.cs
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
/// A NosTale client base object.
/// </summary>
public class NtClient : NostaleObject
{
    /// <summary>
    /// Create <see cref="NtClient"/> instance.
    /// </summary>
    /// <param name="nosBrowserManager">The NosTale process browser.</param>
    /// <param name="options">The options.</param>
    /// <returns>The player manager or an error.</returns>
    public static Result<NtClient> Create(NosBrowserManager nosBrowserManager, NtClientOptions options)
    {
        var networkObjectAddress = nosBrowserManager.Scanner.FindPattern(options.NtClientPattern);
        if (!networkObjectAddress.Found)
        {
            return new BindingNotFoundError(options.NtClientPattern, "NtClient");
        }

        if (nosBrowserManager.Process.MainModule is null)
        {
            return new NotFoundError("Cannot find the main module of the target process.");
        }

        var staticAddress = (nuint)(nosBrowserManager.Process.MainModule.BaseAddress + networkObjectAddress.Offset);
        return new NtClient(nosBrowserManager.Memory, staticAddress, options.NtClientOffsets);
    }

    private readonly IMemory _memory;
    private readonly int _staticNtClientAddress;
    private readonly int[] _ntClientOffsets;

    private NtClient(IMemory memory, nuint staticNtClientAddress, int[] ntClientOffsets)
        : base(memory, nuint.Zero)
    {
        _memory = memory;
        _staticNtClientAddress = (int)staticNtClientAddress;
        _ntClientOffsets = ntClientOffsets;
    }

    /// <summary>
    /// Gets the address to the player manager.
    /// </summary>
    public override nuint Address => _memory.FollowStaticAddressOffsets
        (_staticNtClientAddress, _ntClientOffsets);

    /// <summary>
    /// Gets the encryption key used for world encryption.
    /// </summary>
    public int EncryptionKey
    {
        get
        {
            _memory.SafeRead(Address + 0x48, out int encryptionKey);
            return encryptionKey;
        }
    }
}