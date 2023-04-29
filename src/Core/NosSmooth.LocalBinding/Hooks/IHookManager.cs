//
//  IHookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A manager holding all NosTale hooks with actions to execute on all of them.
/// </summary>
public interface IHookManager
{
    /// <summary>
    /// A name of packet send hook.
    /// </summary>
    public const string PacketSendName = "NetworkManager.PacketSend";

    /// <summary>
    /// A name of packet receive hook.
    /// </summary>
    public const string PacketReceiveName = "NetworkManager.PacketReceive";

    /// <summary>
    /// A name of character walk hook.
    /// </summary>
    public const string CharacterWalkName = "CharacterManager.Walk";

    /// <summary>
    /// A name of pet walk hook.
    /// </summary>
    public const string PetWalkName = "PetManager.Walk";

    /// <summary>
    /// A name of entity follow hook.
    /// </summary>
    public const string EntityFollowName = "CharacterManager.EntityFollow";

    /// <summary>
    /// A name of entity unfollow hook.
    /// </summary>
    public const string EntityUnfollowName = "CharacterManager.EntityUnfollow";

    /// <summary>
    /// A name of entity focus hook.
    /// </summary>
    public const string EntityFocusName = "UnitManager.EntityFocus";

    /// <summary>
    /// A name of periodic hook.
    /// </summary>
    public const string PeriodicName = "Periodic";

    /// <summary>
    /// Gets the packet send hook.
    /// </summary>
    public Optional<IPacketSendHook> PacketSend { get; }

    /// <summary>
    /// Gets the packet receive hook.
    /// </summary>
    public Optional<IPacketReceiveHook> PacketReceive { get; }

    /// <summary>
    /// Gets the player walk hook.
    /// </summary>
    public Optional<IPlayerWalkHook> PlayerWalk { get; }

    /// <summary>
    /// Gets the entity follow hook.
    /// </summary>
    public Optional<IEntityFollowHook> EntityFollow { get; }

    /// <summary>
    /// Gets the entity unfollow hook.
    /// </summary>
    public Optional<IEntityUnfollowHook> EntityUnfollow { get; }

    /// <summary>
    /// Gets the player walk hook.
    /// </summary>
    public Optional<IPetWalkHook> PetWalk { get; }

    /// <summary>
    /// Gets the entity focus hook.
    /// </summary>
    public Optional<IEntityFocusHook> EntityFocus { get; }

    /// <summary>
    /// Gets the periodic function hook.
    /// </summary>
    /// <remarks>
    /// May be any function that is called periodically.
    /// This is used for synchronizing using <see cref="NosThreadSynchronizer"/>.
    /// </remarks>
    public Optional<IPeriodicHook> Periodic { get; }

    /// <summary>
    /// Gets all of the hooks.
    /// </summary>
    public IReadOnlyList<INostaleHook> Hooks { get; }

    /// <summary>
    /// Initializes all hooks.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <returns>A result that may or may not have failed.</returns>
    public Result Initialize(NosBindingManager bindingManager, NosBrowserManager browserManager);

    /// <summary>
    /// Enable hooks from the given list.
    /// </summary>
    /// <remarks>
    /// Use constants from <see cref="IHookManager"/>,
    /// such as IHookManager.PacketSendName.
    /// </remarks>
    /// <param name="names">The hooks to enable.</param>
    public void Enable(IEnumerable<string> names);

    /// <summary>
    /// Disable hooks from the given list.
    /// </summary>
    /// <remarks>
    /// Use constants from <see cref="IHookManager"/>,
    /// such as IHookManager.PacketSendName.
    /// </remarks>
    /// <param name="names">The hooks to disable.</param>
    public void Disable(IEnumerable<string> names);

    /// <summary>
    /// Disable all hooks.
    /// </summary>
    public void DisableAll();

    /// <summary>
    /// Enable all hooks.
    /// </summary>
    public void EnableAll();

    /// <summary>
    /// Checks whether hook of the given type is loaded (there were no errors in finding the function).
    /// </summary>
    /// <typeparam name="THook">The type of the hook.</typeparam>
    /// <returns>Whether the hook is loaded/present.</returns>
    public bool IsHookLoaded<THook>()
        where THook : INostaleHook;

    /// <summary>
    /// Checks whether hook of the given type is loaded (there were no errors in finding the function)
    /// and that the wrapper function is present/usable.
    /// </summary>
    /// <typeparam name="THook">The type of the hook.</typeparam>
    /// <returns>Whether the hook is loaded/present and usable.</returns>
    public bool IsHookUsable<THook>()
        where THook : INostaleHook;

    /// <summary>
    /// Checks whether hook of the given type is loaded (there were no errors in finding the function).
    /// </summary>
    /// <param name="hookType">The type of the hook.</typeparam>
    /// <returns>Whether the hook is loaded/present.</returns>
    public bool IsHookLoaded(Type hookType);

    /// <summary>
    /// Checks whether hook of the given type is loaded (there were no errors in finding the function)
    /// and that the wrapper function is present/usable.
    /// </summary>
    /// <param name="hookType">The type of the hook.</typeparam>
    /// <returns>Whether the hook is loaded/present and usable.</returns>
    public bool IsHookUsable(Type hookType);
}