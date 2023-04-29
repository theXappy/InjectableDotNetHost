//
//  SingleHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using NosSmooth.Extensions.SharedBinding.EventArgs;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using Remora.Results;

namespace NosSmooth.Extensions.SharedBinding.Hooks;

/// <summary>
/// A hook for a single instance of NosSmooth sharing with the rest of application.
/// </summary>
/// <typeparam name="TFunction">The function delegate.</typeparam>
/// <typeparam name="TWrapperFunction">A wrapper function that abstracts the call to original function. May get the neccessary object to call the function and accept only relevant arguments.</typeparam>
/// <typeparam name="TEventArgs">The event args used in case of a call.</typeparam>
public class SingleHook<TFunction, TWrapperFunction, TEventArgs> : INostaleHook<TFunction, TWrapperFunction, TEventArgs>
    where TFunction : Delegate
    where TWrapperFunction : Delegate
    where TEventArgs : System.EventArgs
{
    private readonly INostaleHook<TFunction, TWrapperFunction, TEventArgs> _underlyingHook;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleHook{TFunction, TWrapperFunction, TEventArgs}"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public SingleHook(INostaleHook<TFunction, TWrapperFunction, TEventArgs> underlyingHook)
    {
        _underlyingHook = underlyingHook;
    }

    /// <summary>
    /// Called upon Enable or Disable.
    /// </summary>
    public event EventHandler<HookStateEventArgs>? StateChanged;

    /// <inheritdoc />
    public bool IsUsable => _underlyingHook.IsUsable;

    /// <inheritdoc />
    public string Name => _underlyingHook.Name;

    /// <inheritdoc />
    public bool IsEnabled { get; private set; }

    /// <inheritdoc />
    public Result Enable()
    {
        if (!IsEnabled)
        {
            IsEnabled = true;
            StateChanged?.Invoke(this, new HookStateEventArgs(true));
            _underlyingHook.Called += FireCalled;
        }

        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public Result Disable()
    {
        if (IsEnabled)
        {
            IsEnabled = true;
            StateChanged?.Invoke(this, new HookStateEventArgs(false));
            _underlyingHook.Called -= FireCalled;
        }

        return Result.FromSuccess();
    }

    private void FireCalled(object? owner, TEventArgs eventArgs)
    {
        Called?.Invoke(this, eventArgs);
    }

    /// <inheritdoc />
    public Optional<TWrapperFunction> WrapperFunction => _underlyingHook.WrapperFunction;

    /// <inheritdoc />
    public TFunction OriginalFunction => _underlyingHook.OriginalFunction;

    /// <inheritdoc />
    public event EventHandler<TEventArgs>? Called;
}