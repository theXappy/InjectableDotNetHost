//
//  EntityUnfollowHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <inheritdoc />
internal class EntityUnfollowHook : SingleHook<IEntityUnfollowHook.EntityUnfollowDelegate,
    IEntityUnfollowHook.EntityUnfollowWrapperDelegate, EntityEventArgs>, IEntityUnfollowHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUnfollowHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public EntityUnfollowHook(INostaleHook<IEntityUnfollowHook.EntityUnfollowDelegate, IEntityUnfollowHook.EntityUnfollowWrapperDelegate, EntityEventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}