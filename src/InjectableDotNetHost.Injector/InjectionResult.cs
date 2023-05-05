//
//  InjectionResult.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace InjectableDotNetHost.Injector;

/// <summary>
/// A result obtained from bootstrap.
/// </summary>
public enum InjectionResult
{
    /// <summary>
    /// Hostfxr.dll was not found, is .NET installed? It should be as this is a .NET application injecting the dll...
    /// </summary>
    HostfxrNotFound = 0,

    /// <summary>
    /// A runtimeconfig.json of the assembly to be injected was not found.
    /// </summary>
    /// <remarks>
    /// Be sure to include <GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>
    /// in a library that will be injected.
    /// </remarks>
    RuntimeConfigNotFound = 1,

    /// <summary>
    /// The specified class or type was not found.
    /// </summary>
    /// <remarks>
    /// Be sure to put it in this format: "namespace.type.method, assembly",
    /// see samples.
    /// </remarks>
    ClassOrMethodNotFound = 2
}