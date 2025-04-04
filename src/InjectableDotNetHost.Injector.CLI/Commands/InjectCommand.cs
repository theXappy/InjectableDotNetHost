﻿//
//  InjectCommand.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace InjectableDotNetHost.Injector.CLI.Commands
{
    /// <summary>
    /// Injection command for injecting .NET 5+ libraries with UnmanagedCallersOnly method.
    /// </summary>
    internal class InjectCommand : CommandGroup
    {
        private readonly DotNetHostInjector _injector;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectCommand"/> class.
        /// </summary>
        /// <param name="injector">The injector.</param>
        public InjectCommand(DotNetHostInjector injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// The command to inject.
        /// </summary>
        /// <param name="process">The id of the process or part of its name.</param>
        /// <param name="dllPath">The path to the dll to inject.</param>
        /// <param name="typeName">The full type specifier. Default is LibraryName.DllMain, LibraryName.</param>
        /// <param name="methodName">The name of the UnmanagedCallersOnly method. Default is Main.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("inject")]
        public Task<Result> Inject
        (
            [Description("The id of the process to inject into.")]
            string process,
            [Description("The path to the dll to inject.")]
            string dllPath,
            [Option('t', "type"), Description("The full type specifier. Default is LibraryName.DllMain, LibraryName")]
            string? typeName = null,
            [Option('m', "method"), Description("The name of the UnmanagedCallersOnly method. Default is Main")]
            string? methodName = null
        )
        {
            if (!int.TryParse(process, out var processId))
            {
                var foundProcess = Process.GetProcesses().FirstOrDefault
                    (x => x.ProcessName.Contains(process, StringComparison.OrdinalIgnoreCase));
                if (foundProcess is null)
                {
                    return Task.FromResult(Result.FromError(new NotFoundError("Could not find the given process.")));
                }

                processId = foundProcess.Id;
            }

            var dllName = Path.GetFileNameWithoutExtension(dllPath);
            var result = _injector.Inject
            (
                processId,
                dllPath,
                typeName ?? $"{dllName}.DllMain, {dllName}",
                methodName ?? "Main",
                new byte[] { 1, 2, 3, 4, 5 }
            );
            if (result != 0)
            {
                return Task.FromResult(Result.FromError<int>(result));
            }

            Console.WriteLine($"Got {result} from the managed injected dll.");
            return Task.FromResult(Result.FromSuccess());
        }
    }
}