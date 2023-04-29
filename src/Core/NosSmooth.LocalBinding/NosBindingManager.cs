//
//  NosBindingManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Hooks.Implementations;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Options;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Helpers;
using Reloaded.Hooks.Definitions.Internal;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Hooks.Tools;
using Reloaded.Hooks.X86;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding;

/// <summary>
/// Nostale entity binding manager.
/// </summary>
public class NosBindingManager : IDisposable
{
    private readonly NosBrowserManager _browserManager;
    private readonly IHookManager _hookManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosBindingManager"/> class.
    /// </summary>
    /// <param name="browserManager">The NosTale browser manager.</param>
    /// <param name="hookManager">The hook manager.</param>
    public NosBindingManager
    (
        NosBrowserManager browserManager,
        IHookManager hookManager
    )
    {
        _browserManager = browserManager;
        _hookManager = hookManager;
        Hooks = new ReloadedHooks();
        Memory = new Memory();
        Scanner = new Scanner(Process.GetCurrentProcess(), Process.GetCurrentProcess().MainModule);
    }

    /// <summary>
    /// Gets the memory scanner.
    /// </summary>
    internal Scanner Scanner { get; }

    /// <summary>
    /// Gets the reloaded hooks.
    /// </summary>
    internal IReloadedHooks Hooks { get; }

    /// <summary>
    /// Gets the current process memory.
    /// </summary>
    internal IMemory Memory { get; }

    /// <summary>
    /// Initialize the existing bindings and hook NosTale functions.
    /// </summary>
    /// <returns>A result that may or may not have succeeded.</returns>
    public IResult Initialize()
    {
        List<IResult> errorResults = new List<IResult>();
        var browserInitializationResult = _browserManager.Initialize();
        if (!browserInitializationResult.IsSuccess)
        {
            if (browserInitializationResult.Error is NotNostaleProcessError)
            {
                return browserInitializationResult;
            }

            errorResults.Add(browserInitializationResult);
        }

        var hookManagerInitializationResult = _hookManager.Initialize(this, _browserManager);
        if (!hookManagerInitializationResult.IsSuccess)
        {
            errorResults.Add(hookManagerInitializationResult);
        }

        return errorResults.Count switch
        {
            0 => Result.FromSuccess(),
            1 => errorResults[0],
            _ => (Result)new AggregateError(errorResults)
        };
    }

    /// <summary>
    /// Gets whether a hook or browser module is present.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <returns>Whether the module is present.</returns>
    public bool IsModulePresent<TModule>()
        => _hookManager.IsHookUsable(typeof(TModule)) || _browserManager.IsModuleLoaded(typeof(TModule));

    /// <inheritdoc />
    public void Dispose()
    {
        Scanner.Dispose();
        _hookManager.DisableAll();
    }

    /// <summary>
    /// Create a hook object for the given pattern.
    /// </summary>
    /// <param name="name">The name of the binding.</param>
    /// <param name="callbackFunction">The callback function to call instead of the original one.</param>
    /// <param name="options">The options for the function hook. (pattern, offset, whether to activate).</param>
    /// <typeparam name="TFunction">The type of the function.</typeparam>
    /// <returns>The hook object or an error.</returns>
    internal Result<IHook<TFunction>> CreateHookFromPattern<TFunction>
    (
        string name,
        TFunction callbackFunction,
        HookOptions options
    )
    {
        var walkFunctionAddress = Scanner.FindPattern(options.MemoryPattern);
        if (!walkFunctionAddress.Found)
        {
            return new BindingNotFoundError(options.MemoryPattern, name);
        }

        try
        {
            var hook = Hooks.CreateHook
            (
                callbackFunction,
                walkFunctionAddress.Offset + (int)_browserManager.Process.MainModule!.BaseAddress + options.Offset
            );
            if (options.Hook)
            {
                hook.Activate();
            }

            return Result<IHook<TFunction>>.FromSuccess(hook);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Create custom assembler hook.
    /// </summary>
    /// <remarks>
    /// Sometimes there are more requirements than Reloaded-Hooks handles
    /// (or maybe I am just configuring it correctly).
    ///
    /// For these cases this method is here. It adds a detour call at the beginning
    /// of a function. The detour function should return 1 to continue,
    /// 0 to return at the beginning.
    /// </remarks>
    /// <param name="name">The name of the binding.</param>
    /// <param name="callbackFunction">The callback function to call instead of the original one.</param>
    /// <param name="options">The options for the function hook. (pattern, offset, whether to activate).</param>
    /// <param name="cancellable">Whether the call may be cancelled.</param>
    /// <typeparam name="TFunction">The type of the function.</typeparam>
    /// <returns>The hook object or an error.</returns>
    internal Result<NosAsmHook<TFunction>>
        CreateCustomAsmHookFromPattern<TFunction>
        (
            string name,
            TFunction callbackFunction,
            HookOptions options,
            bool cancellable = true
        )
        where TFunction : Delegate
    {
        var walkFunctionAddress = Scanner.FindPattern(options.MemoryPattern);
        if (!walkFunctionAddress.Found)
        {
            return new BindingNotFoundError(options.MemoryPattern, name);
        }

        try
        {
            var address = walkFunctionAddress.Offset + (int)_browserManager.Process.MainModule!.BaseAddress
                + options.Offset;
            var wrapper = Hooks.CreateFunction<TFunction>(address);
            var reverseWrapper = Hooks.CreateReverseWrapper<TFunction>(callbackFunction);
            var callDetour = Utilities.GetAbsoluteCallMnemonics
                (reverseWrapper.WrapperPointer.ToUnsigned(), IntPtr.Size == 8);
            if (!Misc.TryGetAttribute<TFunction, FunctionAttribute>(out var attribute))
            {
                return new ArgumentInvalidError(nameof(TFunction), "The function does not have a function attribute.");
            }
            var stackArgumentsCount = Utilities.GetNumberofParameters<TFunction>() - attribute.SourceRegisters.Length;
            if (stackArgumentsCount < 0)
            {
                stackArgumentsCount = 0;
            }

            var asmInstructions = new List<string>();

            asmInstructions.AddRange(new[]
            {
                "use32",
                "pushad",
                "pushfd",
            });

            for (int i = 0; i < stackArgumentsCount; i++)
            { // TODO make it into asm loop
                asmInstructions.AddRange(new[]
                {
                    $"add esp, {36 + (4 * stackArgumentsCount)}",
                    "pop ebx",
                    "sub esp, 0x04",
                    $"sub esp, {36 + (4 * stackArgumentsCount)}",
                    "push ebx",
                });
            }

            asmInstructions.Add(callDetour);

            if (cancellable)
            {
                asmInstructions.AddRange
                (
                    new[]
                    {
                        // check result
                        // 1 means continue executing
                        // 0 means do not permit the call
                        "test eax, eax",
                        "jnz rest",

                        // returned 0, going to return early
                        "popfd",
                        "popad",

                        $"ret {4 * stackArgumentsCount}", // return early
                    }
                );
            }

            asmInstructions.AddRange(new[]
            {
                // returned 1, going to execute the function
                "rest:",
                "popfd",
                "popad"
            });

            var hook = Hooks.CreateAsmHook(asmInstructions.ToArray(), address);

            if (options.Hook)
            {
                hook.Activate();
            }

            return Result<NosAsmHook<TFunction>>.FromSuccess
                (new NosAsmHook<TFunction>(reverseWrapper, wrapper, hook));
        }
        catch (Exception e)
        {
            return e;
        }
    }
}