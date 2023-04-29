//
//  PetWalkHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of NetworkManager.PetWalk.
/// </summary>
internal class PetWalkHook : CancelableNostaleHook<IPetWalkHook.PetWalkDelegate,
    IPetWalkHook.PetWalkWrapperDelegate, PetWalkEventArgs>, IPetWalkHook
{
    /// <summary>
    /// Create the packet send hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<PetWalkHook> Create
        (NosBindingManager bindingManager, HookOptions<IPetWalkHook> options)
    {
        var hook = CreateHook
        (
            bindingManager,
            () => new PetWalkHook(bindingManager.Memory),
            hook => hook.Detour,
            options
        );

        return hook;
    }

    private IMemory _memory;

    private PetWalkHook(IMemory memory)
    {
        _memory = memory;
    }

    /// <inheritdoc />
    public override string Name => IHookManager.PetWalkName;

    /// <inheritdoc />
    public override Optional<IPetWalkHook.PetWalkWrapperDelegate> WrapperFunction
        => (IPetWalkHook.PetWalkWrapperDelegate)((p, x, y) => OriginalFunction(p.Address, (y << 16) | x) == 1);

    /// <inheritdoc />
    protected override IPetWalkHook.PetWalkDelegate WrapWithCalling(IPetWalkHook.PetWalkDelegate function)
        =>
            (
                petManagerPtr,
                position,
                un0,
                un1,
                un2
            ) =>
            {
                CallingFromNosSmooth = true;
                var res = function
                (
                    petManagerPtr,
                    position,
                    un0,
                    un1,
                    un2
                );
                CallingFromNosSmooth = false;
                return res;
            };

    private nuint Detour
    (
        nuint petManagerPtr,
        int position,
        short un0,
        int un1,
        int un2
    )
    {
        var petManager = new PetManager(_memory, petManagerPtr);
        var walkArgs = new PetWalkEventArgs
            (petManager, (ushort)(position & 0xFFFF), (ushort)((position >> 16) & 0xFFFF));
        return HandleCall(walkArgs);
    }
}