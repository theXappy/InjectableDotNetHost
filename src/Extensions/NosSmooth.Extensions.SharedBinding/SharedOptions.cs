//
//  SharedOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Data.NOSFiles;
using NosSmooth.LocalBinding;
using NosSmooth.PacketSerializer.Packets;

namespace NosSmooth.Extensions.SharedBinding;

/// <summary>
/// Options for <see cref="SharedManager"/>.
/// </summary>
internal class SharedOptions
{
    private Dictionary<Type, ServiceDescriptor> _descriptors = new();

    /// <summary>
    /// Add service descriptor for given type.
    /// </summary>
    /// <param name="descriptor">The service descriptor.</param>
    public void AddDescriptor(ServiceDescriptor descriptor)
    {
        var type = descriptor.ServiceType;
        if (_descriptors.ContainsKey(type))
        {
            return;
        }

        _descriptors[type] = descriptor;
    }

    /// <summary>
    /// Get descriptor for the given type.
    /// </summary>
    /// <param name="type">The type of the descriptor.</param>
    /// <returns>A descriptor.</returns>
    public ServiceDescriptor GetDescriptor(Type type)
    {
        return _descriptors[type];
    }
}