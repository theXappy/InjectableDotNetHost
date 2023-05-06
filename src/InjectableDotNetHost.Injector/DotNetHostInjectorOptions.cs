//
//  DotNetHostInjectorOptions.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace InjectableDotNetHost.Injector
{
    /// <summary>
    /// Options for DotNetHostInjector.
    /// </summary>
    public class DotNetHostInjectorOptions : IOptions<DotNetHostInjectorOptions>
    {
        /// <summary>
        /// Gets or sets the path to the bootstrap dll.
        /// </summary>
        /// <remarks>
        /// If not absolute path, then relative path from the current executing process is assumed.
        /// </remarks>
        public string BootstrapPath_x86 { get; set; } = "cpp_dll/x86/InjectableDotNetHost.Bootstrap_x86.dll";

        /// <summary>
        /// Gets or sets the path to the bootstrap dll.
        /// </summary>
        /// <remarks>
        /// If not absolute path, then relative path from the current executing process is assumed.
        /// </remarks>
        public string BootstrapPath_x64 { get; set; } = "cpp_dll/x64/InjectableDotNetHost.Bootstrap_x64.dll";

        /// <inheritdoc/>
        public DotNetHostInjectorOptions Value => this;
    }
}
