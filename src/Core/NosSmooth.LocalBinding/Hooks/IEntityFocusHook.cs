//
//  IEntityFocusHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks.Definitions.X86;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of UnitManager.EntityFocus.
/// </summary>
public interface IEntityFocusHook : INostaleHook<IEntityFocusHook.EntityFocusDelegate,
    IEntityFocusHook.EntityFocusWrapperDelegate, EntityEventArgs>
{
    /// <summary>
    /// NosTale entity focus function to hook.
    /// </summary>
    /// <param name="unitManagerPtr">The unit manager object.</param>
    /// <param name="entityPtr">The entity object.</param>
    /// <returns>1 to proceed to NosTale function, 0 to block the call.</returns>
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    public delegate nuint EntityFocusDelegate(nuint unitManagerPtr, nuint entityPtr);

    /// <summary>
    /// Entity focus function.
    /// </summary>
    /// <remarks>
    /// In case entity is null, unfocus any entity.
    /// </remarks>
    /// <param name="entity">The entity object.</param>
    public delegate void EntityFocusWrapperDelegate(MapBaseObj? entity);
}