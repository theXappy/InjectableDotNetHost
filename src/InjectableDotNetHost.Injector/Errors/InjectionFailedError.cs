//
//  InjectionFailedError.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace InjectableDotNetHost.Injector.Errors;

/// <summary>
/// The injection could not be finished successfully.
/// </summary>
/// <param name="DllPath">The path to the dll.</param>
/// <param name="Reason">The reason.</param>
/// <param name="Result">The possible results.</param>
public record InjectionFailedError(string DllPath, string Reason, InjectionResult? Result = null) : ResultError
    ($"Could not inject {DllPath} dll into the process. {Reason}, {Result}");