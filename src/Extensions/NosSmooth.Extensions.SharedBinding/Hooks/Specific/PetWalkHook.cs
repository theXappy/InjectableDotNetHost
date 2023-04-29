//
//  PetWalkHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <inheritdoc />
internal class PetWalkHook : SingleHook<IPetWalkHook.PetWalkDelegate, IPetWalkHook.PetWalkWrapperDelegate, PetWalkEventArgs>, IPetWalkHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PetWalkHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public PetWalkHook(INostaleHook<IPetWalkHook.PetWalkDelegate, IPetWalkHook.PetWalkWrapperDelegate, PetWalkEventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}