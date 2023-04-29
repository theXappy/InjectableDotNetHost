//
//  DllMain.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace WalkCommands;

/// <summary>
/// Represents the dll entrypoint class.
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
        AllocConsole();
        new Thread(() =>
        {
            try
            {
                new Startup().RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }).Start();
        return 0;
    }
}