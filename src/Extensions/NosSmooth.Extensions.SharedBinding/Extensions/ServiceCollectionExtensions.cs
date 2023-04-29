//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NosSmooth.Data.NOSFiles;
using NosSmooth.Extensions.SharedBinding.Hooks;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Extensions;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.PacketSerializer.Packets;

namespace NosSmooth.Extensions.SharedBinding.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds shared <see cref="IHookManager"/>.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection ShareHooks(this IServiceCollection serviceCollection)
    {
        var originalHookManager = serviceCollection
            .Last(x => x.ServiceType == typeof(IHookManager));

        var sharedHookManager = ServiceDescriptor.Singleton<SharedHookManager>
        (
            p =>
            {
                var sharedHookManager = p.GetRequiredService<SharedManager>().GetShared<IHookManager>(p);
                return new SharedHookManager(sharedHookManager);
            }
        );

        return serviceCollection
            .Configure<SharedOptions>(o => o.AddDescriptor(originalHookManager))
            .Configure<SharedOptions>(o => o.AddDescriptor(sharedHookManager))
            .AddSingleton<SharedHookManager>(p => SharedManager.Instance.GetShared<SharedHookManager>(p))
            .Replace(ServiceDescriptor.Singleton<IHookManager, SingleHookManager>());
    }

    /// <summary>
    /// Replaces <typeparamref name="T"/>
    /// with shared equivalent. That allows for multiple programs injected inside NosTale.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <typeparam name="T">The shared type.</typeparam>
    /// <returns>The same collection.</returns>
    public static IServiceCollection Share<T>(this IServiceCollection serviceCollection)
        where T : class
    {
        var original = serviceCollection
            .Last(x => x.ServiceType == typeof(T));

        return serviceCollection
            .Configure<SharedOptions>(o => o.AddDescriptor(original))
            .Replace
            (
                ServiceDescriptor.Singleton<T>(p => SharedManager.Instance.GetShared<T>(p))
            );
    }

    /// <summary>
    /// Tries to replace <see cref="T"/>
    /// with shared equivalent. That allows for multiple programs injected inside NosTale.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <typeparam name="T">The shared type.</typeparam>
    /// <returns>The same collection.</returns>
    public static IServiceCollection TryShare<T>(this IServiceCollection serviceCollection)
        where T : class
    {
        if (serviceCollection.Any(x => x.ServiceType == typeof(T)))
        {
            return serviceCollection.Share<T>();
        }

        return serviceCollection;
    }

    /// <summary>
    /// Replaces some NosSmooth types with their shared equivalents.
    /// That allows for multiple programs injected inside NosTale.
    /// </summary>
    /// <param name="serviceCollection">The collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection ShareNosSmooth(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<SharedManager>(p => SharedManager.Instance)
            .ShareHooks()
            .TryShare<NosBrowserManager>()
            .TryShare<IPacketTypesRepository>()
            .TryShare<NostaleDataFilesManager>();
    }
}