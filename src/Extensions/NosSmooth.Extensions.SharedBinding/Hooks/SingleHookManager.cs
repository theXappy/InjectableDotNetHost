//
//  SingleHookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Hooks;

/// <summary>
/// A hook manager for a single NosSmooth instance using shared data.
/// </summary>
public class SingleHookManager : IHookManager
{
    private readonly SharedHookManager _sharedHookManager;
    private readonly HookManagerOptions _options;
    private Dictionary<string, INostaleHook> _hooks;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleHookManager"/> class.
    /// </summary>
    /// <param name="sharedHookManager">The shared hook manager.</param>
    /// <param name="options">The hook options.</param>
    public SingleHookManager(SharedHookManager sharedHookManager, IOptions<HookManagerOptions> options)
    {
        _hooks = new Dictionary<string, INostaleHook>();
        _sharedHookManager = sharedHookManager;
        _options = options.Value;
    }

    /// <inheritdoc />
    public Optional<IPacketSendHook> PacketSend => GetHook<IPacketSendHook>(IHookManager.PacketSendName);

    /// <inheritdoc />
    public Optional<IPacketReceiveHook> PacketReceive => GetHook<IPacketReceiveHook>(IHookManager.PacketReceiveName);

    /// <inheritdoc />
    public Optional<IPlayerWalkHook> PlayerWalk => GetHook<IPlayerWalkHook>(IHookManager.CharacterWalkName);

    /// <inheritdoc />
    public Optional<IEntityFollowHook> EntityFollow => GetHook<IEntityFollowHook>(IHookManager.EntityFollowName);

    /// <inheritdoc />
    public Optional<IEntityUnfollowHook> EntityUnfollow => GetHook<IEntityUnfollowHook>(IHookManager.EntityUnfollowName);

    /// <inheritdoc />
    public Optional<IPetWalkHook> PetWalk => GetHook<IPetWalkHook>(IHookManager.PetWalkName);

    /// <inheritdoc />
    public Optional<IEntityFocusHook> EntityFocus => GetHook<IEntityFocusHook>(IHookManager.EntityFocusName);

    /// <inheritdoc />
    public Optional<IPeriodicHook> Periodic => GetHook<IPeriodicHook>(IHookManager.PeriodicName);

    /// <inheritdoc />
    public IReadOnlyList<INostaleHook> Hooks => _hooks.Values.ToList();

    /// <inheritdoc />
    public Result Initialize(NosBindingManager bindingManager, NosBrowserManager browserManager)
    {
        _initialized = true;
        var (hooks, result) = _sharedHookManager.InitializeInstance(bindingManager, browserManager, _options);
        _hooks = hooks;
        return result;
    }

    /// <inheritdoc />
    public void Enable(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var hook = GetHook<INostaleHook>(name);
            hook.TryDo(h => h.Enable());
        }
    }

    /// <inheritdoc />
    public void Disable(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var hook = GetHook<INostaleHook>(name);
            hook.TryDo(h => h.Disable());
        }
    }

    /// <inheritdoc />
    public void DisableAll()
    {
        foreach (var hook in _hooks.Values)
        {
            hook.Disable();
        }
    }

    /// <inheritdoc />
    public void EnableAll()
    {
        foreach (var hook in _hooks.Values)
        {
            hook.Enable();
        }
    }

    /// <inheritdoc/>
    public bool IsHookLoaded<THook>()
        where THook : INostaleHook
        => IsHookLoaded(typeof(THook));

    /// <inheritdoc/>
    public bool IsHookUsable<THook>()
        where THook : INostaleHook
        => IsHookUsable(typeof(THook));

    /// <inheritdoc/>
    public bool IsHookLoaded(Type hookType)
        => GetHook(hookType).IsPresent;

    /// <inheritdoc/>
    public bool IsHookUsable(Type hookType)
        => GetHook(hookType).TryGet(out var h) && h.IsUsable;

    private Optional<T> GetHook<T>(string name)
        where T : INostaleHook
    {
        if (!_initialized)
        {
            throw new InvalidOperationException
                ($"Could not load hook {typeof(T)}. Did you forget to call IHookManager.Initialize?");
        }

        var hook = _hooks.Values.FirstOrDefault(x => x is T);
        if (hook is not T typed)
        {
            return Optional<T>.Empty;
        }

        return typed;
    }

    private Optional<INostaleHook> GetHook(Type hookType)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException
                ($"Could not load hook {hookType.Name}. Did you forget to call IHookManager.Initialize?");
        }

        var hook = _hooks.Values.FirstOrDefault(hookType.IsInstanceOfType);
        if (hook is null)
        {
            return Optional<INostaleHook>.Empty;
        }

        return new Optional<INostaleHook>(hook);
    }
}