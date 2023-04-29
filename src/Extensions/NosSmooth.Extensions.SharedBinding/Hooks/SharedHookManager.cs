//
//  SharedHookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Extensions.SharedBinding.Hooks.Specific;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Hooks;

/// <summary>
/// A hook manager managing <see cref="SingleHookManager"/>s of all of the instances.
/// </summary>
public class SharedHookManager
{
    private readonly IHookManager _underlyingManager;

    private bool _initialized;
    private Dictionary<string, int> _hookedCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedHookManager"/> class.
    /// </summary>
    /// <param name="underlyingManager">The underlying hook manager.</param>
    public SharedHookManager
    (
        IHookManager underlyingManager
    )
    {
        _hookedCount = new Dictionary<string, int>();
        _underlyingManager = underlyingManager;
    }

    /// <summary>
    /// Initialize a shared NosSmooth instance.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The initial options to be respected.</param>
    /// <returns>The dictionary containing all of the hooks.</returns>
    public (Dictionary<string, INostaleHook>, Result) InitializeInstance
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookManagerOptions options)
    {
        Result result = Result.FromSuccess();
        if (!_initialized)
        {
            result = _underlyingManager.Initialize(bindingManager, browserManager);
            _initialized = true;
        }

        var hooks = new Dictionary<string, INostaleHook>();

        // TODO: initialize using reflection
        HandleAdd
        (
            hooks,
            IHookManager.PeriodicName,
            InitializeSingleHook
            (
                _underlyingManager.Periodic,
                u => new PeriodicHook(u),
                options.PeriodicHook
            )
        );

        HandleAdd
        (
            hooks,
            IHookManager.EntityFocusName,
            InitializeSingleHook
            (
                _underlyingManager.EntityFocus,
                u => new EntityFocusHook(u),
                options.EntityFocusHook
            )
        );

        HandleAdd
        (
            hooks,
            IHookManager.EntityFollowName,
            InitializeSingleHook
            (
                _underlyingManager.EntityFollow,
                u => new EntityFollowHook(u),
                options.EntityFollowHook
            )
        );

        HandleAdd
        (
            hooks,
            IHookManager.EntityUnfollowName,
            InitializeSingleHook
            (
                _underlyingManager.EntityUnfollow,
                u => new EntityUnfollowHook(u),
                options.EntityUnfollowHook
            )
        );

        HandleAdd
        (
            hooks,
            IHookManager.PacketReceiveName,
            InitializeSingleHook
            (
                _underlyingManager.PacketReceive,
                u => new PacketReceiveHook(u),
                options.PacketReceiveHook
            )
        );

        HandleAdd
        (
            hooks,
            IHookManager.PacketSendName,
            InitializeSingleHook
            (
                _underlyingManager.PacketSend,
                u => new PacketSendHook(u),
                options.PacketSendHook
            )
        );

        HandleAdd
        (
            hooks,
            IHookManager.PetWalkName,
            InitializeSingleHook
            (
                _underlyingManager.PetWalk,
                u => new PetWalkHook(u),
                options.PetWalkHook
            )
        );

        HandleAdd
        (
            hooks,
            IHookManager.CharacterWalkName,
            InitializeSingleHook
            (
                _underlyingManager.PlayerWalk,
                u => new PlayerWalkHook(u),
                options.PlayerWalkHook
            )
        );

        return (hooks, result);
    }

    private INostaleHook<TFunction, TWrapperFunction, TEventArgs>? InitializeSingleHook<THook, TFunction,
        TWrapperFunction,
        TEventArgs>
    (
        Optional<THook> hookOptional,
        Func<THook, SingleHook<TFunction, TWrapperFunction, TEventArgs>> hookCreator,
        HookOptions options
    )
        where THook : notnull
        where TFunction : Delegate
        where TWrapperFunction : Delegate
        where TEventArgs : System.EventArgs
    {
        if (!hookOptional.TryGet(out var underlyingHook))
        {
            return null;
        }

        var hook = hookCreator(underlyingHook);
        hook.StateChanged += (_, state) =>
        {
            if (!_hookedCount.ContainsKey(hook.Name))
            {
                _hookedCount[hook.Name] = 0;
            }

            _hookedCount[hook.Name] += state.Enabled ? 1 : -1;

            if (state.Enabled)
            {
                _underlyingManager.Enable(new[] { hook.Name });
            }
            else if (_hookedCount[hook.Name] == 0)
            {
                _underlyingManager.Disable(new[] { hook.Name });
            }
        };

        if (options.Hook)
        {
            hook.Enable();
        }

        return hook;
    }

    private void HandleAdd(Dictionary<string, INostaleHook> hooks, string name, INostaleHook? hook)
    {
        if (hook is not null)
        {
            hooks.Add(name, hook);
        }
    }
}