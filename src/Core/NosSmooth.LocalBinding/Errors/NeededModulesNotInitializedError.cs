//
//  NeededModulesNotInitializedError.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.LocalBinding.Errors;

/// <summary>
/// A modules that are needed for the given operation are not loaded.
/// </summary>
/// <param name="additionalMessage">The message to show.</param>
/// <param name="modules">The modules.</param>
public record NeededModulesNotInitializedError
    (
        string additionalMessage,
        params string[] modules
    )
    : ResultError($"{additionalMessage}. Some needed modules ({string.Join(", ", modules)}) are not loaded");