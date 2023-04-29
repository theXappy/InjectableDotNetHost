//
//  IEntityUnfollowHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using Reloaded.Hooks.Definitions.X86;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of CharacterManager.EntityFollow.
/// </summary>
public interface IEntityUnfollowHook : INostaleHook<IEntityUnfollowHook.EntityUnfollowDelegate,
    IEntityUnfollowHook.EntityUnfollowWrapperDelegate, EntityEventArgs>
{
    /// <summary>
    /// NosTale entity follow function to hook.
    /// </summary>
    /// <param name="playerManagerPtr">The player manager object.</param>
    /// <param name="unknown">Unknown. TODO.</param>
    /// <returns>1 to proceed to NosTale function, 0 to block the call.</returns>
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    public delegate nuint EntityUnfollowDelegate(nuint playerManagerPtr, int unknown = 0);

    /// <summary>
    /// Entity unfollow function.
    /// </summary>
    public delegate void EntityUnfollowWrapperDelegate();
}