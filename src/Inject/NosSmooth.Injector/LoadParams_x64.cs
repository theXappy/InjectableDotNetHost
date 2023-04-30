//
//  LoadParams_x64.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.Injector;

/// <summary>
/// The parameters passed to the inject module.
/// </summary>
internal struct LoadParams_x64
{
    /// <summary>
    /// The full path of the library.
    /// </summary>
    public long LibraryPath;

    /// <summary>
    /// The full path of the library.
    /// </summary>
    public long RuntimeConfigPath;

    /// <summary>
    /// The full path to the type with the method marked as UnsafeCallersOnly.
    /// </summary>
    /// <remarks>
    /// Can be for example "LibraryNamespace.Type, LibraryNamespace".
    /// </remarks>
    public long TypePath;

    /// <summary>
    /// The name of the method to execute.
    /// </summary>
    public long MethodName;

    /// <summary>
    /// The user data to pass.
    /// </summary>
    public long UserData;
}