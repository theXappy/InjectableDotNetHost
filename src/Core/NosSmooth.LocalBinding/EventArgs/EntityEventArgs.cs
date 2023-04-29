//
//  EntityEventArgs.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using NosSmooth.LocalBinding.Structs;

namespace NosSmooth.LocalBinding.EventArgs;

/// <summary>
/// Event args with an entity stored in <see cref="MapBaseObj"/>.
/// </summary>
public class EntityEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets the entity.
    /// </summary>
    public MapBaseObj? Entity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityEventArgs"/> class.
    /// </summary>
    /// <param name="entity">The entity.</param>
    public EntityEventArgs(MapBaseObj? entity)
    {
        Entity = entity;
    }
}