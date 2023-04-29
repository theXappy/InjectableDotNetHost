//
//  DllMain.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace SimplePiiBot;

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
    [UnmanagedCallersOnly(EntryPoint = "Main")]
    public static int Main(nuint data)
    {
        Thread.Sleep(10_000);
        AllocConsole();
        new Thread
        (
            () =>
            {
                try
                {
                    Console.WriteLine("WIN!");
                    //MainEntry().GetAwaiter().GetResult();
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