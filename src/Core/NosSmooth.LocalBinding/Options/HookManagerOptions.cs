//
//  HookManagerOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="IHookManager"/>.
/// </summary>
public class HookManagerOptions
{
    /// <summary>
    /// Gets or sets the configuration for player walk function hook.
    /// </summary>
    public HookOptions<IPlayerWalkHook> PlayerWalkHook { get; set; }
        = new(IHookManager.CharacterWalkName, false, "55 8B EC 83 C4 EC 53 56 57 66 89 4D FA", 0);

    /// <summary>
    /// Gets or sets the configuration for entity follow function hook.
    /// </summary>
    public HookOptions<IEntityFollowHook> EntityFollowHook { get; set; }
        = new(IHookManager.EntityFollowName, false, "55 8B EC 51 53 56 57 88 4D FF 8B F2 8B F8", 0);

    /// <summary>
    /// Gets or sets the configuration for entity unfollow function hook.
    /// </summary>
    public HookOptions<IEntityUnfollowHook> EntityUnfollowHook { get; set; }
        = new(IHookManager.EntityUnfollowName, false, "80 78 14 00 74 1A", 0);

    /// <summary>
    /// Gets or sets the configuration for packet receive function hook.
    /// </summary>
    public HookOptions<IPacketReceiveHook> PacketReceiveHook { get; set; }
        = new
        (
            IHookManager.PacketReceiveName,
            true,
            "55 8B EC 83 C4 ?? 53 56 57 33 C9 89 4D ?? 89 4D ?? 89 55 ?? 8B D8 8B 45 ??",
            0
        );

    /// <summary>
    /// Gets or sets the configuration for packet send function hook.
    /// </summary>
    public HookOptions<IPacketSendHook> PacketSendHook { get; set; }
        = new(IHookManager.PacketSendName, true, "53 56 8B F2 8B D8 EB 04", 0);

    /// <summary>
    /// Gets or sets the configuration for pet walk function hook.
    /// </summary>
    public HookOptions<IPetWalkHook> PetWalkHook { get; set; }
        = new(IHookManager.PetWalkName, false, "55 8b ec 83 c4 e4 53 56 57 8b f9 89 55 fc 8b d8 c6 45 fb 00", 0);

    /// <summary>
    /// Gets or sets the configuration for any periodic function hook.
    /// </summary>
    public HookOptions<IPeriodicHook> PeriodicHook { get; set; }
        = new(IHookManager.PeriodicName, true, "55 8B EC 53 56 83 C4", 0);

    /// <summary>
    /// Gets or sets the configuration for entity focus function hook.
    /// </summary>
    public HookOptions<IEntityFocusHook> EntityFocusHook { get; set; }
        = new(IHookManager.EntityFocusName, false, "73 00 00 00 55 8b ec b9 05 00 00 00", 4);
}