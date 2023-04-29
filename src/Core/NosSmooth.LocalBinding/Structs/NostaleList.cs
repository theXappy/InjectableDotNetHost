//
//  NostaleList.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ObjectiveC;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding.Structs;

/// <summary>
/// A class representing a list from nostale.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public abstract class NostaleList<T> : NostaleObject, IEnumerable<T>
    where T : NostaleObject
{
    private readonly IMemory _memory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleList{T}"/> class.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="objListPointer">The object list pointer.</param>
    public NostaleList(IMemory memory, nuint objListPointer)
        : base(memory, objListPointer)
    {
        _memory = memory;
    }

    /// <summary>
    /// Gets the element at the given index.
    /// </summary>
    /// <param name="index">The index of the element.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is not in the bounds of the array.</exception>
    public T this[int index]
    {
        get
        {
            if (index >= Length || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            _memory.SafeRead(Address + 0x04, out int arrayAddress);
            _memory.SafeRead((nuint)arrayAddress + (nuint)(0x04 * index), out int objectAddress);

            return CreateNew(_memory, (nuint)objectAddress);
        }
    }

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    public int Length
    {
        get
        {
            _memory.SafeRead(Address + 0x08, out int length);
            return length;
        }
    }

    /// <summary>
    /// Create a new instance of <typeparamref name="T"/> with the given memory and address.
    /// </summary>
    /// <param name="memory">The memory.</param>
    /// <param name="address">The address.</param>
    /// <returns>The new object.</returns>
    protected abstract T CreateNew(IMemory memory, nuint address);

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        return new NostaleListEnumerator(this);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class NostaleListEnumerator : IEnumerator<T>
    {
        private readonly NostaleList<T> _list;
        private int _index;

        public NostaleListEnumerator(NostaleList<T> list)
        {
            _index = -1;
            _list = list;
        }

        public bool MoveNext()
        {
            if (_list.Length > _index + 1)
            {
                _index++;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public T Current => _list[_index];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}