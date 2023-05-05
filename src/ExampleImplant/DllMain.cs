//
//  DllMain.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace ExampleImplant;

/// <summary>
/// The entrypoint class.
/// </summary>
public class DllMain
{
    /// <summary>
    /// Allocate console.
    /// </summary>
    /// <returns>Whether the operation was successful.</returns>
    [DllImport("kernel32")]
    public static extern bool AllocConsole();

    /// <summary>
    /// Represents the dll entrypoint method.
    /// </summary>
    /// <param name="data">Data passed from injector.</param>
    /// <returns>Return code.</returns>
    [UnmanagedCallersOnly(EntryPoint = "Main")]
    public static int Main(nuint data)
    {
        AllocConsole();
        new Thread
        (
            () =>
            {
                try
                {
                    Console.WriteLine("WIN!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        ).Start();
        return 0;
    }
}