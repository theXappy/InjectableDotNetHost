//
//  NosThreadSynchronizerOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.LocalBinding.Options;

/// <summary>
/// Options for <see cref="NosThreadSynchronizer"/>.
/// </summary>
public class NosThreadSynchronizerOptions
{
    /// <summary>
    /// Gets or sets the number of max tasks per one iteration.
    /// </summary>
    public int MaxTasksPerIteration { get; set; } = 10;
}