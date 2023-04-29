//
//  CancelableNostaleHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using NosSmooth.LocalBinding.Extensions;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of a NosTale function
/// that may be cancelled (not propagated to NosTale) when fired.
/// </summary>
/// <typeparam name="TFunction">The function delegate.</typeparam>
/// <typeparam name="TWrapperFunction">A wrapper function that abstracts the call to original function. May get the neccessary object to call the function and accept only relevant arguments.</typeparam>
/// <typeparam name="TEventArgs">The event args used in case of a call.</typeparam>
public abstract class CancelableNostaleHook<TFunction, TWrapperFunction, TEventArgs> : INostaleHook<TFunction, TWrapperFunction, TEventArgs>
    where TFunction : Delegate
    where TWrapperFunction : Delegate
    where TEventArgs : CancelEventArgs
{
    /// <summary>
    /// Creates the hook instance.
    /// </summary>
    /// <param name="bindingManager">The binding manager to create the hook.</param>
    /// <param name="new">Create new hook object.</param>
    /// <param name="detour">The function that obtains detour.</param>
    /// <param name="options">The options.</param>
    /// <typeparam name="T">The hook type.</typeparam>
    /// <returns>The hook, or failed result.</returns>
    protected static Result<T> CreateHook<T>(NosBindingManager bindingManager, Func<T> @new, Func<T, TFunction> detour, HookOptions options)
        where T : CancelableNostaleHook<TFunction, TWrapperFunction, TEventArgs>
    {
        var nosHook = @new();

        var hookResult = bindingManager.CreateCustomAsmHookFromPattern<TFunction>
            (nosHook.Name, detour(nosHook), options);
        if (!hookResult.IsDefined(out var hook))
        {
            return Result<T>.FromError(hookResult);
        }

        nosHook.Hook = hook;
        return nosHook;
    }

    private TFunction? _originalFunction;

    /// <summary>
    /// Gets the hook.
    /// </summary>
    protected NosAsmHook<TFunction> Hook { get; set; } = null!;

    /// <summary>
    /// Set to true at start of WrapWithCalling, set to false at end of WrapWithCalling.
    /// </summary>
    protected bool CallingFromNosSmooth { get; set; }

    /// <inheritdoc />
    public bool IsEnabled => Hook.Hook.IsEnabled;

    /// <inheritdoc />
    public abstract Optional<TWrapperFunction> WrapperFunction { get; }

    /// <inheritdoc/>
    public TFunction OriginalFunction
    {
        get
        {
            if (_originalFunction is null)
            {
                _originalFunction = WrapWithCalling(Hook.OriginalFunction.GetWrapper());
            }

            return _originalFunction;
        }
    }

    /// <inheritdoc />
    public event EventHandler<TEventArgs>? Called;

    /// <inheritdoc />
    public bool IsUsable => WrapperFunction.IsPresent;

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public Result Enable()
    {
        Hook.Hook.EnableOrActivate();
        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public Result Disable()
    {
        Hook.Hook.Disable();
        return Result.FromSuccess();
    }

    /// <summary>
    /// Wrap the target function with setting CallingFromNosSmooth to true.
    /// </summary>
    /// <example>
    /// protected MyFun WrapWithCalling(MyFun fun) {
    ///     return (a, b) => {
    ///       CallingFromNosSmooth = true;
    ///       var res = fun(a, b);
    ///       CallingFromNosSmooth = false;
    ///       return res;
    ///     }
    /// }.
    /// </example>
    /// <param name="function">The function to wrap.</param>
    /// <returns>The wrapped function.</returns>
    protected abstract TFunction WrapWithCalling(TFunction function);

    /// <summary>
    /// Calls the event, returns whether to proceed.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    /// <returns>Whether to proceed.</returns>
    protected nuint HandleCall(TEventArgs args)
    {
        if (CallingFromNosSmooth)
        { // this is a call from NosSmooth, do not invoke the event.
            return 1;
        }

        Called?.Invoke(this, args);
        return args.Cancel ? 0 : (nuint)1;
    }
}