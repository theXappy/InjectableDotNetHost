//
//  NosAsmHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Reloaded.Hooks.Definitions;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// An assembly hook data.
/// </summary>
/// <param name="ReverseWrapper">The reverse wrapper.</param>
/// <param name="OriginalFunction">The original function.</param>
/// <param name="Hook">The hook.</param>
public record NosAsmHook<TFunction>
(
    IReverseWrapper<TFunction> ReverseWrapper,
    IFunction<TFunction> OriginalFunction,
    IAsmHook Hook
);