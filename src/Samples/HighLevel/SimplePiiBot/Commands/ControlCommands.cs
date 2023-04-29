//
//  ControlCommands.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using OneOf.Types;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace SimplePiiBot.Commands;

/// <summary>
/// Commands for controlling the bot.
/// </summary>
public class ControlCommands : CommandGroup
{
    private readonly Bot _bot;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlCommands"/> class.
    /// </summary>
    /// <param name="bot">The bot.</param>
    public ControlCommands(Bot bot)
    {
        _bot = bot;
    }

    /// <summary>
    /// Handle the start command.
    /// </summary>
    /// <returns>A result that may or may not succeed.</returns>
    [Command("start")]
    public async Task<Result> HandleStartAsync()
        => await _bot.StartAsync(CancellationToken);

    /// <summary>
    /// Handle the stop command.
    /// </summary>
    /// <returns>A result that may or may not succeed.</returns>
    [Command("stop")]
    public async Task<Result> HandleStopAsync()
        => await _bot.StopAsync(CancellationToken);
}