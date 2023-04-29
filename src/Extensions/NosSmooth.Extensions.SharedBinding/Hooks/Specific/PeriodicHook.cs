//
//  PeriodicHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;

namespace NosSmooth.Extensions.SharedBinding.Hooks.Specific;

/// <summary>
/// A hook of a periodic function,
/// preferably called every frame.
/// </summary>
internal class PeriodicHook :
    SingleHook<IPeriodicHook.PeriodicDelegate, IPeriodicHook.PeriodicDelegate, System.EventArgs>, IPeriodicHook
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PeriodicHook"/> class.
    /// </summary>
    /// <param name="underlyingHook">The underlying hook.</param>
    public PeriodicHook(INostaleHook<IPeriodicHook.PeriodicDelegate, IPeriodicHook.PeriodicDelegate, System.EventArgs> underlyingHook)
        : base(underlyingHook)
    {
    }
}