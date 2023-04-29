//
//  NostaleObject.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Reloaded.Memory.Sources;

namespace NosSmooth.LocalBinding.Structs;

/// <summary>
/// A NosTale object base.
/// </summary>
public abstract class NostaleObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleObject"/> class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="address">The address in the memory.</param>
    protected NostaleObject(IMemory memory, nuint address)
    {
        Memory = memory;
        Address = address;
    }

    /// <summary>
    /// Gets the memory the object is stored in.
    /// </summary>
    internal virtual IMemory Memory { get; set; } = null!;

    /// <summary>
    /// Gets the address of the object.
    /// </summary>
    public virtual nuint Address { get; internal set; } = nuint.Zero;
}