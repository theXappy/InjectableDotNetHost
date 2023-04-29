//
//  INostaleHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of a NosTale function.
/// </summary>
/// <typeparam name="TFunction">The function delegate.</typeparam>
/// <typeparam name="TWrapperFunction">A wrapper function that abstracts the call to original function. May get the neccessary object to call the function and accept only relevant arguments.</typeparam>
/// <typeparam name="TEventArgs">The event args used in case of a call.</typeparam>
public interface INostaleHook<TFunction, TWrapperFunction, TEventArgs> : INostaleHook
    where TFunction : Delegate
    where TWrapperFunction : Delegate
    where TEventArgs : System.EventArgs
{
    /// <summary>
    /// Gets the wrapper function delegate.
    /// </summary>
    public Optional<TWrapperFunction> WrapperFunction { get; }

    /// <summary>
    /// Gets the original function delegate.
    /// </summary>
    public TFunction OriginalFunction { get; }

    /// <summary>
    /// An event fired in case the function is called from NosTale.
    /// </summary>
    public event EventHandler<TEventArgs>? Called;
}

/// <summary>
/// A hook of a NosTale function.
/// </summary>
public interface INostaleHook
{
    /// <summary>
    /// Gets whether the wrapper function is present and usable.
    /// </summary>
    public bool IsUsable { get; }

    /// <summary>
    /// Gets the name of the function.
    /// </summary>
    /// <remarks>
    /// Usually denoted as Type.Method,
    /// for example PlayerManager.Walk.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Gets whether the hook is currently enabled.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// Enable the hook.
    /// </summary>
    /// <remarks>
    /// If it already is enabled, does nothing.
    /// </remarks>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result Enable();

    /// <summary>
    /// Disable the hook.
    /// </summary>
    /// <remarks>
    /// If it already is disabled, does nothing.
    /// </remarks>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result Disable();
}