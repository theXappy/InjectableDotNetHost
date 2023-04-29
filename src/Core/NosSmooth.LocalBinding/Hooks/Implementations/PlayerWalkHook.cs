//
//  PlayerWalkHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of NetworkManager.PlayerWalk.
/// </summary>
internal class PlayerWalkHook : CancelableNostaleHook<IPlayerWalkHook.WalkDelegate,
    IPlayerWalkHook.WalkWrapperDelegate, WalkEventArgs>, IPlayerWalkHook
{
    /// <summary>
    /// Create the packet send hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<PlayerWalkHook> Create
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookOptions<IPlayerWalkHook> options)
    {
        var hook = CreateHook
        (
            bindingManager,
            () => new PlayerWalkHook(browserManager.PlayerManager),
            hook => hook.Detour,
            options
        );

        return hook;
    }

    private readonly Optional<PlayerManager> _playerManager;

    private PlayerWalkHook(Optional<PlayerManager> playerManager)
    {
        _playerManager = playerManager;
    }

    /// <inheritdoc />
    public override string Name => IHookManager.CharacterWalkName;

    /// <inheritdoc />
    public override Optional<IPlayerWalkHook.WalkWrapperDelegate> WrapperFunction
        => _playerManager.Map<IPlayerWalkHook.WalkWrapperDelegate>
        (
            playerManager => (x, y) =>
            {
                var playerManagerObject = playerManager.Address;
                return OriginalFunction(playerManagerObject, (y << 16) | x) == 1;
            }
        );

    /// <inheritdoc />
    protected override IPlayerWalkHook.WalkDelegate WrapWithCalling(IPlayerWalkHook.WalkDelegate function)
        =>
            (
                playerManagerPtr,
                position,
                un0,
                un1
            ) =>
            {
                CallingFromNosSmooth = true;
                var res = function(playerManagerPtr, position, un0, un1);
                CallingFromNosSmooth = false;
                return res;
            };

    private nuint Detour
    (
        nuint playerManagerPtr,
        int position,
        short un0,
        int un1
    )
    {
        var walkArgs = new WalkEventArgs((ushort)(position & 0xFFFF), (ushort)((position >> 16) & 0xFFFF));
        return HandleCall(walkArgs);
    }
}