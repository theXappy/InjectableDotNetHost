//
//  EntityFocusHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <inheritdoc />
internal class EntityFocusHook : SingleHook<IEntityFocusHook.EntityFocusDelegate,
    IEntityFocusHook.EntityFocusWrapperDelegate, EntityEventArgs>, IEntityFocusHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFocusHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public EntityFocusHook(INostaleHook<IEntityFocusHook.EntityFocusDelegate, IEntityFocusHook.EntityFocusWrapperDelegate, EntityEventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}