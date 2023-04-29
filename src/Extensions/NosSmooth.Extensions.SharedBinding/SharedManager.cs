//
//  SharedManager.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NosSmooth.Data.NOSFiles;
using NosSmooth.LocalBinding;
using NosSmooth.PacketSerializer.Packets;

namespace NosSmooth.Extensions.SharedBinding;

/// <summary>
/// Manager for sharing <see cref="NosBindingManager"/>,
/// <see cref="NostaleDataFilesManager"/> and
/// <see cref="IPacketTypesRepository"/>.
/// </summary>
public class SharedManager
{
    private static SharedManager? _instance;
    private Dictionary<Type, object> _sharedData = new();

    /// <summary>
    /// A singleton instance.
    /// One per process.
    /// </summary>
    public static SharedManager Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = new SharedManager();
            }

            return _instance;
        }
    }

    private SharedManager()
    {
    }

    /// <summary>
    /// Get shared equivalent of the given type.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <typeparam name="T">The type to get shared instance of.</typeparam>
    /// <returns>The shared instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown in case the type is not shared.</exception>
    public T GetShared<T>(IServiceProvider services)
        where T : class
    {
        if (!_sharedData.ContainsKey(typeof(T)))
        {
            _sharedData[typeof(T)] = CreateShared<T>(services);
        }

        return (T)_sharedData[typeof(T)];
    }

    private T CreateShared<T>(IServiceProvider services)
        where T : class
    {
        var options = services.GetRequiredService<IOptions<SharedOptions>>();
        var descriptor = options.Value.GetDescriptor(typeof(T));

        if (descriptor is null)
        {
            throw new InvalidOperationException
                ($"Could not find {typeof(T)} in the service provider when trying to make a shared instance.");
        }

        if (descriptor.ImplementationInstance is not null)
        {
            return (T)descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory is not null)
        {
            return (T)descriptor.ImplementationFactory(services);
        }

        if (descriptor.ImplementationType is not null)
        {
            return (T)ActivatorUtilities.CreateInstance(services, descriptor.ImplementationType);
        }

        return ActivatorUtilities.CreateInstance<T>(services);
    }
}