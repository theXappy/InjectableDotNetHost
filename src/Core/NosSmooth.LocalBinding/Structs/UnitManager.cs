//
//  UnitManager.cs
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
public class UnitManager : NostaleObject
{
        /// <summary>
    /// Create <see cref="PlayerManager"/> instance.
    /// </summary>
    /// <param name="nosBrowserManager">The NosTale process browser.</param>
    /// <param name="options">The options.</param>
    /// <returns>The player manager or an error.</returns>
    public static Result<UnitManager> Create(NosBrowserManager nosBrowserManager, UnitManagerOptions options)
    {
        var unitObjectAddress = nosBrowserManager.Scanner.FindPattern(options.UnitManagerPattern);
        if (!unitObjectAddress.Found)
        {
            return new BindingNotFoundError(options.UnitManagerPattern, "UnitManager");
        }

        if (nosBrowserManager.Process.MainModule is null)
        {
            return new NotFoundError("Cannot find the main module of the target process.");
        }

        var staticAddress = (int)(nosBrowserManager.Process.MainModule.BaseAddress + unitObjectAddress.Offset);
        return new UnitManager(nosBrowserManager.Memory, staticAddress, options.UnitManagerOffsets);
    }

    private readonly IMemory _memory;
    private readonly int _staticUnitManagerAddress;
    private readonly int[] _optionsUnitManagerOffsets;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitManager"/> class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="staticUnitManagerAddress">The pointer to the beginning of the player manager structure.</param>
    /// <param name="optionsUnitManagerOffsets">The unit manager offsets.</param>
    public UnitManager(IMemory memory, int staticUnitManagerAddress, int[] optionsUnitManagerOffsets)
        : base(memory, (nuint)staticUnitManagerAddress)
    {
        _memory = memory;
        _staticUnitManagerAddress = staticUnitManagerAddress;
        _optionsUnitManagerOffsets = optionsUnitManagerOffsets;
    }

    /// <summary>
    /// Gets the address to the player manager.
    /// </summary>
    public override nuint Address => _memory.FollowStaticAddressOffsets
        (_staticUnitManagerAddress, _optionsUnitManagerOffsets);
}