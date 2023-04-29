//
//  EntityUnfollowHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of NetworkManager.EntityUnfollow.
/// </summary>
internal class EntityUnfollowHook : CancelableNostaleHook<IEntityUnfollowHook.EntityUnfollowDelegate,
    IEntityUnfollowHook.EntityUnfollowWrapperDelegate, EntityEventArgs>, IEntityUnfollowHook
{
    /// <summary>
    /// Create the packet send hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<EntityUnfollowHook> Create
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookOptions<IEntityUnfollowHook> options)
    {
        var hook = CreateHook
        (
            bindingManager,
            () => new EntityUnfollowHook(browserManager.PlayerManager),
            hook => hook.Detour,
            options
        );

        return hook;
    }

    private readonly Optional<PlayerManager> _playerManager;

    private EntityUnfollowHook(Optional<PlayerManager> playerManager)
    {
        _playerManager = playerManager;
    }

    /// <inheritdoc />
    public override string Name => IHookManager.EntityUnfollowName;

    /// <inheritdoc />
    public override Optional<IEntityUnfollowHook.EntityUnfollowWrapperDelegate> WrapperFunction
    => _playerManager.Map<IEntityUnfollowHook.EntityUnfollowWrapperDelegate>(playerManager => () => OriginalFunction(playerManager.Address));

    /// <inheritdoc />
    protected override IEntityUnfollowHook.EntityUnfollowDelegate WrapWithCalling(IEntityUnfollowHook.EntityUnfollowDelegate function)
        => (playerManagerPtr, un) =>
            {
                CallingFromNosSmooth = true;
                var res = function(playerManagerPtr, un);
                CallingFromNosSmooth = false;
                return res;
            };

    private nuint Detour
    (
        nuint playerManagerPtr,
        int unknown = 0
    )
        => HandleCall(new EntityEventArgs(null));
}