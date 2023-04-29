//
//  PeriodicHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NosSmooth.LocalBinding.Extensions;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <inheritdoc />
internal class PeriodicHook : IPeriodicHook
{
    /// <summary>
    /// Create the periodic hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<PeriodicHook> Create(NosBindingManager bindingManager, HookOptions<IPeriodicHook> options)
    {
        var periodicHook = new PeriodicHook();

        var hookResult = bindingManager.CreateCustomAsmHookFromPattern<IPeriodicHook.PeriodicDelegate>
            (periodicHook.Name, periodicHook.Detour, options, false);
        if (!hookResult.IsDefined(out var hook))
        {
            return Result<PeriodicHook>.FromError(hookResult);
        }

        periodicHook._hook = hook;
        return periodicHook;
    }

    private PeriodicHook()
    {
    }

    private NosAsmHook<IPeriodicHook.PeriodicDelegate> _hook = null!;

    /// <inheritdoc/>
    public bool IsUsable => true;

    /// <inheritdoc />
    public string Name => IHookManager.PeriodicName;

    /// <inheritdoc />
    public bool IsEnabled => _hook.Hook.IsEnabled;

    /// <inheritdoc />
    public Optional<IPeriodicHook.PeriodicDelegate> WrapperFunction => Optional<IPeriodicHook.PeriodicDelegate>.Empty;

    /// <inheritdoc/>
    public IPeriodicHook.PeriodicDelegate OriginalFunction => throw new InvalidOperationException
        ("Calling NosTale periodic function from NosSmooth is not allowed.");

    /// <inheritdoc />
    public event EventHandler<System.EventArgs>? Called;

    /// <inheritdoc />
    public Result Enable()
    {
        _hook.Hook.EnableOrActivate();
        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public Result Disable()
    {
        _hook.Hook.Disable();
        return Result.FromSuccess();
    }

    private void Detour()
    {
        Called?.Invoke(this, System.EventArgs.Empty);
    }
}