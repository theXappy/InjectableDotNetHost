//
//  EntityFollowHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <inheritdoc />
internal class EntityFollowHook : SingleHook<IEntityFollowHook.EntityFollowDelegate,
    IEntityFollowHook.EntityFollowWrapperDelegate, EntityEventArgs>, IEntityFollowHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFollowHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public EntityFollowHook(INostaleHook<IEntityFollowHook.EntityFollowDelegate, IEntityFollowHook.EntityFollowWrapperDelegate, EntityEventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}