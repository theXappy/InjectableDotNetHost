//
//  MapBaseList.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Reloaded.Memory.Sources;

namespace NosSmooth.LocalBinding.Structs;

/// <inheritdoc />
public class MapBaseList : NostaleList<MapBaseObj>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapBaseList"/> class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="objListPointer">The list pointer.</param>
    public MapBaseList(IMemory memory, nuint objListPointer)
        : base(memory, objListPointer)
    {
    }

    /// <inheritdoc />
    protected override MapBaseObj CreateNew(IMemory memory, nuint address)
    {
        return new MapBaseObj(memory, address);
    }
}