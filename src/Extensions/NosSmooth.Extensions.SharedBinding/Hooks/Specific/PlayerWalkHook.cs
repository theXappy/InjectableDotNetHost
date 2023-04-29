//
//  PlayerWalkHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <inheritdoc />
internal class PlayerWalkHook : SingleHook<IPlayerWalkHook.WalkDelegate, IPlayerWalkHook.WalkWrapperDelegate, WalkEventArgs>, IPlayerWalkHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerWalkHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public PlayerWalkHook(INostaleHook<IPlayerWalkHook.WalkDelegate, IPlayerWalkHook.WalkWrapperDelegate, WalkEventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}