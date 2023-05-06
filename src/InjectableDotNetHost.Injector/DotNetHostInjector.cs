//
//  DotNetHostInjector.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using InjectableDotNetHost.Injector.Errors;
using Microsoft.Extensions.Options;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace InjectableDotNetHost.Injector;

/// <summary>
/// .NET Core Host injector for .NET 5+ projects.
/// </summary>
/// <remarks>
/// Your entry point method should be a static UnmanagedCallersOnly method.
/// </remarks>
public class DotNetHostInjector
{
    private readonly DotNetHostInjectorOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DotNetHostInjector"/> class.
    /// </summary>
    /// <param name="options">The injector options.</param>
    public DotNetHostInjector(IOptions<DotNetHostInjectorOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Inject the given .NET 5+ dll into a process.
    /// </summary>
    /// <remarks>
    /// The dll must also have .runtimeconfig.json present next to the dll.
    ///
    /// The method you want to call has to be marked with UnmanagedCallersOnly,
    /// has to be the default convention, return an integer (that will be returned out of this method)
    /// and accept one argument. That argument should accept a pointer do byte array containing data from <paramref name="data"/>.
    /// One possibility is to use <see cref="nuint"/>.
    ///
    /// Parsing data is up to the user, no format is specified, the array length is not passed to the
    /// injected dll, so you better know the size prior to calling or store the size in the beginning of the array.
    /// </remarks>
    /// <param name="processId">The id of the process to inject to.</param>
    /// <param name="dllPath">The absolute path to the dll to inject.</param>
    /// <param name="classPath">The full path to the class. Such as "MyLibrary.DllMain, MyLibrary".</param>
    /// <param name="methodName">The name of the method to execute. The method should return void and have no parameters.</param>
    /// <param name="data">The data to pass to the process. The array will be allocated inside the target process.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result<int> Inject
    (
        int processId,
        string dllPath,
        string classPath,
        string methodName = "Main",
        byte[]? data = default
    )
    {
        using var process = Process.GetProcessById(processId);
        return Inject
        (
            process,
            dllPath,
            classPath,
            methodName,
            data
        );
    }

    /// <summary>
    /// Checks if a process is 32-bit or 64-bit.
    /// </summary>
    /// <param name="hProcess">Process.</param>
    /// <param name="wow64Process">Is it a Wow64.</param>
    /// <returns>True if the value was read, false otherwise.</returns>
    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWow64Process(
        [In] IntPtr hProcess,
        [Out] out bool wow64Process
    );

    /// <summary>
    /// Inject the given .NET 5+ dll into a process.
    /// </summary>
    /// <remarks>
    /// The dll must also have .runtimeconfig.json present next to the dll.
    ///
    /// The method you want to call has to be marked with UnmanagedCallersOnly,
    /// has to be the default convention, return an integer (that will be returned out of this method)
    /// and accept one argument. That argument should accept a pointer do byte array containing data from <paramref name="data"/>.
    /// One possibility is to use <see cref="nuint"/>.
    ///
    /// Parsing data is up to the user, no format is specified, the array length is not passed to the
    /// injected dll, so you better know the size prior to calling or store the size in the beginning of the array.
    /// </remarks>
    /// <param name="process">The process to inject to.</param>
    /// <param name="dllPath">The absolute path to the dll to inject.</param>
    /// <param name="classPath">The full path to the class. Such as "MyLibrary.DllMain, MyLibrary".</param>
    /// <param name="methodName">The name of the method to execute. The method should return void and have no parameters.</param>
    /// <param name="data">The data to pass to the process. The array will be allocated inside the target process.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result<int> Inject
    (
        Process process,
        string dllPath,
        string classPath,
        string methodName = "Main",
        byte[]? data = default
    )
    {
        try
        {
            bool x64 = false;
            if (IsWow64Process(process.Handle, out var isWow64))
            {
                x64 = !isWow64;
            }
            else
            {
                throw new Exception("ERROR: Failed to determine if the process is running as a 32-bit or 64-bit application.");
            }

            dllPath = Path.GetFullPath(dllPath);
            if (!File.Exists(dllPath))
            {
                return new NotFoundError($"Could not find the managed dll file at \"{dllPath}\".");
            }

            using var injector = new Reloaded.Injector.Injector(process);
            var memory = new ExternalMemory(process);

            string absoluteBootstrapPath;
            if (!GetBootstrapDllPath(x64, out absoluteBootstrapPath, out Result<int> error))
            {
                return error;
            }

            var netHostInjectionResult = InjectNetHostDll
            (
                injector,
                Path.GetFullPath("."),
                System.IO.Path.GetDirectoryName(absoluteBootstrapPath)
            );

            if (!netHostInjectionResult.IsSuccess)
            {
                return Result<int>.FromError(netHostInjectionResult);
            }

            var directoryName = Path.GetDirectoryName(dllPath);
            if (directoryName is null)
            {
                return new NotFoundError("There was an error obtaining directory name of the dll path.");
            }

            var runtimePath = Path.Combine
                (directoryName, Path.GetFileNameWithoutExtension(dllPath)) + ".runtimeconfig.json";

            if (!File.Exists(runtimePath))
            {
                return new NotFoundError($"Could not find the runtimeconfig.json file at \"{runtimePath}\".");
            }

            using var dllPathMemory = AllocateString(memory, dllPath);
            using var classPathMemory = AllocateString(memory, classPath);
            using var methodNameMemory = AllocateString(memory, methodName);
            using var runtimePathMemory = AllocateString(memory, runtimePath);
            using var userDataMemory = Allocate(memory, data ?? Array.Empty<byte>());

            if (!dllPathMemory.Allocated || !classPathMemory.Allocated || !methodNameMemory.Allocated
                || !runtimePathMemory.Allocated)
            {
                return new ArgumentNullError("Could not allocate memory in the external process.");
            }

            var injected = injector.Inject(absoluteBootstrapPath);
            if (injected == 0)
            {
                return new GenericError($"InjectionFailedError({absoluteBootstrapPath}, \"Did you forget to copy nethost.dll into the process directory?\")");
            }

            int functionResult;
            if (x64)
            {
                var loadParams64 = new LoadParams_x64
                {
                    LibraryPath = (long)dllPathMemory.Pointer,
                    MethodName = (long)methodNameMemory.Pointer,
                    RuntimeConfigPath = (long)runtimePathMemory.Pointer,
                    TypePath = (long)classPathMemory.Pointer,
                    UserData = (long)userDataMemory.Pointer
                };
                functionResult = injector.CallFunction(absoluteBootstrapPath, "LoadAndCallMethod", loadParams64);
            }
            else
            {
                var loadParams32 = new LoadParams_x32
                {
                    LibraryPath = (int)dllPathMemory.Pointer,
                    MethodName = (int)methodNameMemory.Pointer,
                    RuntimeConfigPath = (int)runtimePathMemory.Pointer,
                    TypePath = (int)classPathMemory.Pointer,
                    UserData = (int)userDataMemory.Pointer
                };
                functionResult = injector.CallFunction(absoluteBootstrapPath, "LoadAndCallMethod", loadParams32);

            }

            injector.Eject(absoluteBootstrapPath);

            if (functionResult < 3)
            {
                return new GenericError($"InjectionFailedError({dllPath}, " +
                                        $"\"Couldn't initialize the nethost or call the main function, did you specify the class and method correctly? Result: {functionResult}\", " +
                                        $"{(InjectionResult)functionResult})");
            }

            return functionResult - 3;
        }
        catch (UnauthorizedAccessException)
        {
            return new GenericError($"InsufficientPermissionsError({process.Id}, {process.ProcessName})");
        }
        catch (SecurityException)
        {
            return new GenericError($"InsufficientPermissionsError({process.Id}, {process.ProcessName})");
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private bool GetBootstrapDllPath(bool x64, out string absoluteBootstrapPath, out Result<int> inject)
    {
        string relativeBootstrapPath = _options.BootstrapPath_x86;
        if (x64)
        {
            relativeBootstrapPath = _options.BootstrapPath_x64;
        }

        absoluteBootstrapPath = Path.GetFullPath(relativeBootstrapPath);
        if (!File.Exists(absoluteBootstrapPath))
        {
            // Try to write the files
            try
            {
                // bootstrap dll
                var dllData =
                    x64
                        ? Properties.Resources.InjectableDotNetHost_Bootstrap_x64dll
                        : Properties.Resources.InjectableDotNetHost_Bootstrap_x86dll;

                // Create the directory path if it does not exist
                string directoryPath = Path.GetDirectoryName(absoluteBootstrapPath);
                Directory.CreateDirectory(directoryPath);

                File.WriteAllBytes(absoluteBootstrapPath, dllData);

                // bootstrap pdb
                string absoluteBoostrapPdbPath = Path.ChangeExtension(absoluteBootstrapPath, "pdb");
                var pdbData =
                    x64
                        ? Properties.Resources.InjectableDotNetHost_Bootstrap_x64pdb
                        : Properties.Resources.InjectableDotNetHost_Bootstrap_x86pdb;
                File.WriteAllBytes(absoluteBoostrapPdbPath, pdbData);

                // nethost
                string absoluteNethostPath = Path.Combine(Path.GetDirectoryName(absoluteBootstrapPath)!, "nethost.dll");
                var nethostData =
                    x64
                        ? Properties.Resources.nethost_x64
                        : Properties.Resources.nethost_x86;
                File.WriteAllBytes(absoluteNethostPath, nethostData);

            }
            catch (Exception ex)
            {
                inject = new Remora.Results.ExceptionError(ex, $"Failed to dump bootstrap dll or pdb to disk. Target path: {absoluteBootstrapPath}");
                return false;
            }

            inject = new NotFoundError($"Could not find the dll to inject at \"{relativeBootstrapPath}\".");
            return false;
        }

        inject = default;
        return true;
    }

    private Result InjectNetHostDll(Reloaded.Injector.Injector injector, params string?[] pathsToSearch)
    {
        string? foundPath = pathsToSearch
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => Path.Combine(x, "nethost.dll"))
            .Select(Path.GetFullPath)
            .FirstOrDefault(File.Exists);

        if (foundPath is null)
        {
            return new NotFoundError
            (
                "Could not find nethost.dll to inject (tried to look in executing process directory, injector directory, target process directory)"
            );
        }

        var handle = injector.Inject(foundPath);

        if (handle == 0)
        {
            return new GenericError($"InjectionFailedError({foundPath}, \"Only the devil knows why this happened.\")");
        }

        return Result.FromSuccess();
    }

    private ManagedMemoryAllocation AllocateString(IMemory memory, string str)
    {
        var bytes = Encoding.Unicode.GetBytes(str);
        var allocated = memory.Allocate(bytes.Length + 1);
        if (allocated == (nuint)0)
        {
            return new ManagedMemoryAllocation(memory, allocated);
        }

        memory.SafeWriteRaw(allocated + (nuint)bytes.Length, new byte[] { 0 });
        memory.SafeWriteRaw(allocated, bytes);
        return new ManagedMemoryAllocation(memory, allocated);
    }

    private ManagedMemoryAllocation Allocate(IMemory memory, byte[] data)
    {
        if (data.Length == 0)
        {
            return new ManagedMemoryAllocation(memory, (nuint)0);
        }

        var allocated = memory.Allocate(data.Length);
        if (allocated == (nuint)0)
        {
            return new ManagedMemoryAllocation(memory, allocated);
        }

        memory.SafeWriteRaw(allocated, data);
        return new ManagedMemoryAllocation(memory, allocated);
    }
}