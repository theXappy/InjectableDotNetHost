//
//  HookManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <inheritdoc />
internal class HookManager : IHookManager
{
    private readonly HookManagerOptions _options;
    private readonly Dictionary<string, INostaleHook> _hooks;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="HookManager"/> class.
    /// </summary>
    /// <param name="options">The hook manager options.</param>
    public HookManager(IOptionsSnapshot<HookManagerOptions> options)
    {
        _options = options.Value;
        _hooks = new Dictionary<string, INostaleHook>();
    }

    /// <inheritdoc/>
    public Optional<IPacketSendHook> PacketSend => GetHook<IPacketSendHook>(IHookManager.PacketSendName);

    /// <inheritdoc/>
    public Optional<IPacketReceiveHook> PacketReceive => GetHook<IPacketReceiveHook>(IHookManager.PacketReceiveName);

    /// <inheritdoc/>
    public Optional<IPlayerWalkHook> PlayerWalk => GetHook<IPlayerWalkHook>(IHookManager.CharacterWalkName);

    /// <inheritdoc/>
    public Optional<IEntityFollowHook> EntityFollow => GetHook<IEntityFollowHook>(IHookManager.EntityFollowName);

    /// <inheritdoc/>
    public Optional<IEntityUnfollowHook> EntityUnfollow => GetHook<IEntityUnfollowHook>
        (IHookManager.EntityUnfollowName);

    /// <inheritdoc/>
    public Optional<IPetWalkHook> PetWalk => GetHook<IPetWalkHook>(IHookManager.PetWalkName);

    /// <inheritdoc/>
    public Optional<IEntityFocusHook> EntityFocus => GetHook<IEntityFocusHook>(IHookManager.EntityFocusName);

    /// <inheritdoc/>
    public Optional<IPeriodicHook> Periodic => GetHook<IPeriodicHook>(IHookManager.PeriodicName);

    /// <inheritdoc/>
    public IReadOnlyList<INostaleHook> Hooks => _hooks.Values.ToList();

    /// <inheritdoc/>
    public Result Initialize(NosBindingManager bindingManager, NosBrowserManager browserManager)
    {
        _initialized = true;
        if (_hooks.Count > 0)
        { // already initialized
            return Result.FromSuccess();
        }

        return HandleResults
        (
            () => PeriodicHook.Create(bindingManager, _options.PeriodicHook).Map(MapHook),
            () => EntityFocusHook.Create(bindingManager, browserManager, _options.EntityFocusHook).Map(MapHook),
            () => EntityFollowHook.Create(bindingManager, browserManager, _options.EntityFollowHook).Map(MapHook),
            () => EntityUnfollowHook.Create(bindingManager, browserManager, _options.EntityUnfollowHook).Map(MapHook),
            () => PlayerWalkHook.Create(bindingManager, browserManager, _options.PlayerWalkHook).Map(MapHook),
            () => PetWalkHook.Create(bindingManager, _options.PetWalkHook).Map(MapHook),
            () => PacketSendHook.Create(bindingManager, browserManager, _options.PacketSendHook).Map(MapHook),
            () => PacketReceiveHook.Create(bindingManager, browserManager, _options.PacketReceiveHook).Map(MapHook)
        );
    }

    private INostaleHook MapHook<T>(T original)
        where T : INostaleHook
    {
        return original;
    }

    private Result HandleResults(params Func<Result<INostaleHook>>[] functions)
    {
        List<IResult> errorResults = new List<IResult>();
        foreach (var func in functions)
        {
            try
            {
                var result = func();
                if (result.IsSuccess)
                {
                    _hooks.Add(result.Entity.Name, result.Entity);
                }
                else
                {
                    errorResults.Add(Result.FromError(result));
                }
            }
            catch (Exception e)
            {
                errorResults.Add((Result)e);
            }
        }

        return errorResults.Count switch
        {
            0 => Result.FromSuccess(),
            1 => (Result)errorResults[0],
            _ => new AggregateError(errorResults)
        };
    }

    /// <inheritdoc/>
    public void Enable(IEnumerable<string> names)
    {
        foreach (var hook in Hooks
            .Where(x => names.Contains(x.Name)))
        {
            hook.Enable();
        }
    }

    /// <inheritdoc/>
    public void Disable(IEnumerable<string> names)
    {
        foreach (var hook in Hooks
            .Where(x => names.Contains(x.Name)))
        {
            hook.Disable();
        }
    }

    /// <inheritdoc/>
    public void DisableAll()
    {
        foreach (var hook in Hooks)
        {
            hook.Disable();
        }
    }

    /// <inheritdoc/>
    public void EnableAll()
    {
        foreach (var hook in Hooks)
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
                ($"Could not load hook {name}. Did you forget to call IHookManager.Initialize?");
        }

        if (!_hooks.ContainsKey(name) || _hooks[name] is not T typed)
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

        var hook = _hooks.Values.FirstOrDefault(x => x.GetType() == hookType);
        if (hook is null)
        {
            return Optional<INostaleHook>.Empty;
        }

        return new Optional<INostaleHook>(hook);
    }
}