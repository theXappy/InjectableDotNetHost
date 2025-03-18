//
//  DotNetHostInjector.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Extensions.Options;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.Structs;

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
    public int Inject
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
    public int Inject
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
            bool x64;
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
                throw new Exception($"Could not find the managed dll file at \"{dllPath}\".");
            }

            using var injector = new Reloaded.Injector.Injector(process);
            var memory = new ExternalMemory(process);

            string absoluteBootstrapPath;
            if (!GetBootstrapDllPath(x64, out absoluteBootstrapPath, out int error))
            {
                return error;
            }

            string absoluteBootstrapDirPath = System.IO.Path.GetDirectoryName(absoluteBootstrapPath)!;
            try
            {
                InjectNetHostDll
                (
                    injector,
                    Path.GetFullPath("."),
                    absoluteBootstrapDirPath
                );

            }
            catch (Exception)
            {
                // ah... wrap it maybe?
                throw;
            }

            var directoryName = Path.GetDirectoryName(dllPath);
            if (directoryName is null)
            {
                throw new Exception("There was an error obtaining directory name of the dll path.");
            }

            var runtimePath = Path.Combine
                (directoryName, Path.GetFileNameWithoutExtension(dllPath)) + ".runtimeconfig.json";

            if (!File.Exists(runtimePath))
            {
                throw new Exception($"Could not find the runtimeconfig.json file at \"{runtimePath}\".");
            }

            var depsPath = Path.Combine
                (directoryName, Path.GetFileNameWithoutExtension(dllPath)) + ".deps.json";

            if (File.Exists(depsPath))
            {
                throw new ArgumentException($"{nameof(DotNetHostInjector)} Does NOT support injecting deps.json. This will lead to a weird behaviour in the injected C++ bootstrap.\n" +
                                             $"Luckily, if you have no external dependencies you can just delete the deps.json and your dll will still be injected. Offending file: \"{depsPath}\".");
            }

            using var dllPathMemory = AllocateString(memory, dllPath);
            using var classPathMemory = AllocateString(memory, classPath);
            using var methodNameMemory = AllocateString(memory, methodName);
            using var runtimePathMemory = AllocateString(memory, runtimePath);
            using var userDataMemory = Allocate(memory, data ?? Array.Empty<byte>());

            if (!dllPathMemory.Allocated || !classPathMemory.Allocated || !methodNameMemory.Allocated
                || !runtimePathMemory.Allocated)
            {
                throw new ArgumentException("Could not allocate memory in the external process.");
            }

            // Make everything in the dir, including the bootstrap/deps/runtime files UWP injectable.
            PermissionsHelper.MakeUwpInjectable(directoryName);
            PermissionsHelper.MakeUwpInjectable(absoluteBootstrapDirPath);

            var injected = injector.Inject(absoluteBootstrapPath);
            if (injected == 0)
            {
                throw new Exception($"InjectionFailedError({absoluteBootstrapPath}, \"Did you forget to copy nethost.dll into the process directory?\")");
            }

            int functionResult;
            if (x64)
            {
                var loadParams64 = new LoadParams_x64
                {
                    LibraryPath = (long)dllPathMemory.Pointer.Address,
                    MethodName = (long)methodNameMemory.Pointer.Address,
                    RuntimeConfigPath = (long)runtimePathMemory.Pointer.Address,
                    TypePath = (long)classPathMemory.Pointer.Address,
                    UserData = (long)userDataMemory.Pointer.Address
                };
                functionResult = injector.CallFunction(absoluteBootstrapPath, "LoadAndCallMethod", loadParams64);
            }
            else
            {
                var loadParams32 = new LoadParams_x32
                {
                    LibraryPath = (int)dllPathMemory.Pointer.Address,
                    MethodName = (int)methodNameMemory.Pointer.Address,
                    RuntimeConfigPath = (int)runtimePathMemory.Pointer.Address,
                    TypePath = (int)classPathMemory.Pointer.Address,
                    UserData = (int)userDataMemory.Pointer.Address
                };
                functionResult = injector.CallFunction(absoluteBootstrapPath, "LoadAndCallMethod", loadParams32);

            }

            injector.Eject(absoluteBootstrapPath);

            if (functionResult < 3)
            {
                throw new Exception($"InjectionFailedError({dllPath}, " +
                                        $"\"Couldn't initialize the nethost or call the main function, did you specify the class and method correctly? Result: {functionResult}\", " +
                                        $"{(InjectionResult)functionResult})");
            }

            return functionResult - 3;
        }
        catch (UnauthorizedAccessException)
        {
            throw new Exception($"InsufficientPermissionsError({process.Id}, {process.ProcessName})");
        }
        catch (SecurityException)
        {
            throw new Exception($"InsufficientPermissionsError({process.Id}, {process.ProcessName})");
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private bool GetBootstrapDllPath(bool x64, out string absoluteBootstrapPath, out int inject)
    {
        string relativeBootstrapPath = _options.BootstrapPath_x86;
        if (x64)
        {
            relativeBootstrapPath = _options.BootstrapPath_x64;
        }

        absoluteBootstrapPath = Path.GetFullPath(relativeBootstrapPath);
        bool bootStrapFound = File.Exists(absoluteBootstrapPath);
        if (!bootStrapFound)
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
                string directoryPath = Path.GetDirectoryName(absoluteBootstrapPath)!;
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
                throw new Exception($"Failed to dump bootstrap dll or pdb to disk. Target path: {absoluteBootstrapPath}", ex);
            }
        }

        inject = default;
        return true;
    }

    private void InjectNetHostDll(Reloaded.Injector.Injector injector, params string?[] pathsToSearch)
    {
        string? foundPath = pathsToSearch
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => Path.Combine(x!, "nethost.dll"))
            .Select(Path.GetFullPath)
            .FirstOrDefault(File.Exists);

        if (foundPath is null)
        {
            throw new Exception
            (
                "Could not find nethost.dll to inject (tried to look in executing process directory, injector directory, target process directory)"
            );
        }

        PermissionsHelper.MakeUwpInjectable(foundPath);

        var handle = injector.Inject(foundPath);

        if (handle == 0)
        {
            throw new Exception($"InjectionFailedError({foundPath}, \"Only the devil knows why this happened.\n" +
                                    $"One option is you're injecting a dll without ALL APPLICATION PACKAGES to a UWP app\")");
        }
    }

    private ManagedMemoryAllocation AllocateString(ExternalMemory memory, string str)
    {
        var bytes = Encoding.Unicode.GetBytes(str);
        var allocated = memory.Allocate((nuint)(bytes.Length + 1));
        if (allocated.Address == (nuint)0)
        {
            return new ManagedMemoryAllocation(memory, allocated);
        }

        memory.SafeWrite(allocated.Address + (nuint)bytes.Length, new byte[] { 0 });
        memory.SafeWrite(allocated.Address, bytes);
        return new ManagedMemoryAllocation(memory, allocated);
    }

    private ManagedMemoryAllocation Allocate(ExternalMemory memory, byte[] data)
    {
        if (data.Length == 0)
        {
            return new ManagedMemoryAllocation(memory, new MemoryAllocation());
        }

        var allocated = memory.Allocate((nuint)data.Length);
        if (allocated.Address == (nuint)0)
        {
            return new ManagedMemoryAllocation(memory, allocated);
        }

        memory.SafeWrite(allocated.Address, data);
        return new ManagedMemoryAllocation(memory, allocated);
    }
}