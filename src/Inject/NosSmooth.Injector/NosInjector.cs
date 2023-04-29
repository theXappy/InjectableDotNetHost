//
//  NosInjector.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Extensions.Options;
using NosSmooth.Injector.Errors;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.Injector;

/// <summary>
/// Nos smooth injector for .NET 5+ projects.
/// </summary>
/// <remarks>
/// If you want to inject your project into NosTale that
/// uses NosSmooth.LocalClient, use this injector.
/// You must expose static UnmanagedCallersOnly method.
/// </remarks>
public class NosInjector
{
    private readonly NosInjectorOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosInjector"/> class.
    /// </summary>
    /// <param name="options">The injector options.</param>
    public NosInjector(IOptions<NosInjectorOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Inject the given .NET 5+ dll into NosTale process.
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
    /// Inject the given .NET 5+ dll into NosTale process.
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
            dllPath = Path.GetFullPath(dllPath);
            if (!File.Exists(dllPath))
            {
                return new NotFoundError($"Could not find the managed dll file at \"{dllPath}\".");
            }

            using var injector = new Reloaded.Injector.Injector(process);
            var memory = new ExternalMemory(process);

            var netHostInjectionResult = InjectNetHostDll
            (
                injector,
                Path.GetFullPath("."),
                System.IO.Path.GetDirectoryName(Path.GetFullPath(dllPath)),
                System.IO.Path.GetDirectoryName(process.MainModule?.FileName)
            );

            if (!netHostInjectionResult.IsSuccess)
            {
                return Result<int>.FromError(netHostInjectionResult);
            }

            var directoryName = Path.GetDirectoryName(dllPath);
            if (directoryName is null)
            {
                return new GenericError("There was an error obtaining directory name of the dll path.");
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
                return new GenericError("Could not allocate memory in the external process.");
            }

            var loadParams = new LoadParams
            {
                LibraryPath = (long)dllPathMemory.Pointer,
                MethodName = (long)methodNameMemory.Pointer,
                RuntimeConfigPath = (long)runtimePathMemory.Pointer,
                TypePath = (long)classPathMemory.Pointer,
                UserData = (long)userDataMemory.Pointer
            };

            var nosSmoothInjectPath = Path.GetFullPath(_options.NosSmoothInjectPath);
            if (!File.Exists(nosSmoothInjectPath))
            {
                return new NotFoundError($"Could not find the dll to inject at \"{_options.NosSmoothInjectPath}\".");
            }

            var injected = injector.Inject(nosSmoothInjectPath);
            if (injected == 0)
            {
                return new InjectionFailedError
                    (nosSmoothInjectPath, "Did you forget to copy nethost.dll into the process directory?");
            }

            var functionResult = injector.CallFunction(nosSmoothInjectPath, "LoadAndCallMethod", loadParams);

            injector.Eject(nosSmoothInjectPath);

            if (functionResult < 3)
            {
                return new InjectionFailedError
                (
                    dllPath,
                    $"Couldn't initialize the nethost or call the main function, did you specify the class and method correctly? Result: {functionResult}",
                    (InjectionResult)functionResult
                );
            }

            return functionResult - 3;
        }
        catch (UnauthorizedAccessException)
        {
            return new InsufficientPermissionsError(process.Id, process.ProcessName);
        }
        catch (SecurityException)
        {
            return new InsufficientPermissionsError(process.Id, process.ProcessName);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private Result InjectNetHostDll(Reloaded.Injector.Injector injector, params string?[] pathsToSearch)
    {
        string? foundPath = pathsToSearch
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => Path.Join(x, "nethost.dll"))
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
            return new InjectionFailedError(foundPath, "Only the devil knows why this happened.");
        }

        return Result.FromSuccess();
    }

    private ManagedMemoryAllocation AllocateString(IMemory memory, string str)
    {
        var bytes = Encoding.Unicode.GetBytes(str);
        var allocated = memory.Allocate(bytes.Length + 1);
        if (allocated == nuint.Zero)
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
            return new ManagedMemoryAllocation(memory, nuint.Zero);
        }

        var allocated = memory.Allocate(data.Length);
        if (allocated == nuint.Zero)
        {
            return new ManagedMemoryAllocation(memory, allocated);
        }

        memory.SafeWriteRaw(allocated, data);
        return new ManagedMemoryAllocation(memory, allocated);
    }
}