//
//  DllMain.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosSmooth.ChatCommands;
using NosSmooth.Core.Extensions;
using NosSmooth.Data.NOSFiles.Extensions;
using NosSmooth.Extensions.Combat.Extensions;
using NosSmooth.Extensions.Pathfinding.Extensions;
using NosSmooth.Extensions.SharedBinding.Extensions;
using NosSmooth.Game.Extensions;
using NosSmooth.LocalClient.Extensions;
using Remora.Commands.Extensions;
using SimplePiiBot.Commands;
using SimplePiiBot.Responders;

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

    /// <summary>
    /// The entrypoint method.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task MainEntry()
    {
        var host = Host.CreateDefaultBuilder()
            .UseConsoleLifetime()
            .ConfigureLogging
            (
                b =>
                {
                    b
                        .ClearProviders()
                        .AddConsole();
                }
            )
            .ConfigureServices
            (
                s =>
                {
                    s.AddNostaleCore()
                        .AddNostaleGame()
                        .AddLocalClient()
                        .AddNostaleDataFiles()
                        .ShareNosSmooth()
                        .AddNostaleCombat()
                        .AddNostalePathfinding()
                        .AddSingleton<Bot>()
                        .AddNostaleChatCommands()
                        .AddGameResponder<EntityJoinedResponder>()
                        .AddCommandTree()
                        .WithCommandGroup<ControlCommands>()
                        .WithCommandGroup<EntityCommands>();
                    s.AddHostedService<HostedService>();
                }
            ).Build();
        await host.RunAsync();
    }
}