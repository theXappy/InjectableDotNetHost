//
//  HookExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Reloaded.Hooks.Definitions;

namespace NosSmooth.LocalBinding.Extensions;

/// <summary>
/// Extensions methods for <see cref="HookExtensions"/>.
/// </summary>
public static class HookExtensions
{
    /// <summary>
    /// Enables the hook if it is active.
    /// Activate it if it wasn't activated.
    /// </summary>
    /// <param name="hook">The hook to enable or activate.</param>
    public static void EnableOrActivate(this IHook hook)
    {
        if (!hook.IsHookActivated)
        {
            hook.Activate();
        }
        else
        {
            hook.Enable();
        }
    }

    /// <summary>
    /// Enables the hook if it is active.
    /// Activate it if it wasn't activated.
    /// </summary>
    /// <param name="hook">The hook to enable or activate.</param>
    public static void EnableOrActivate(this IAsmHook hook)
    {
        // asm hook does not activate if it was already activated.
        hook.Activate();
        hook.Enable();
    }
}