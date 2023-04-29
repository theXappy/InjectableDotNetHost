//
//  IEntityFollowHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks.Definitions.X86;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of CharacterManager.EntityFollow.
/// </summary>
public interface IEntityFollowHook : INostaleHook<IEntityFollowHook.EntityFollowDelegate,
    IEntityFollowHook.EntityFollowWrapperDelegate, EntityEventArgs>
{
    /// <summary>
    /// NosTale entity follow function to hook.
    /// </summary>
    /// <param name="playerManagerPtr">The player manager object.</param>
    /// <param name="entityPtr">The entity object.</param>
    /// <param name="unknown1">Unknown 1. TODO.</param>
    /// <param name="unknown2">Unknown 2. TODO.</param>
    /// <returns>1 to proceed to NosTale function, 0 to block the call.</returns>
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx, FunctionAttribute.Register.ecx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    public delegate nuint EntityFollowDelegate
    (
        nuint playerManagerPtr,
        nuint entityPtr,
        int unknown1 = 0,
        int unknown2 = 1
    );

    /// <summary>
    /// Entity follow function.
    /// </summary>
    /// <remarks>
    /// Does not support unfollow. For unfollow, use <see cref="IEntityUnfollowHook"/>.
    /// </remarks>
    /// <param name="entity">The entity object.</param>
    public delegate void EntityFollowWrapperDelegate(MapBaseObj entity);
}