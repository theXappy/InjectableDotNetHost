//
//  IPeriodicHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Reloaded.Hooks.Definitions.X86;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of a periodic function,
/// preferably called every frame.
/// </summary>
public interface
    IPeriodicHook : INostaleHook<IPeriodicHook.PeriodicDelegate, IPeriodicHook.PeriodicDelegate, System.EventArgs>
{
    /// <summary>
    /// NosTale periodic function to hook.
    /// </summary>
    [Function
    (
        new FunctionAttribute.Register[0],
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    public delegate void PeriodicDelegate();
}