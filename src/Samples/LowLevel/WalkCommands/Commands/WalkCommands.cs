//
//  WalkCommands.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.ChatCommands;
using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Core.Commands.Walking;
using NosSmooth.Core.Extensions;
using NosSmooth.Extensions.Pathfinding;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Structs;
using NosSmooth.Packets.Enums;
using NosSmooth.Packets.Enums.Chat;
using NosSmooth.Packets.Enums.Entities;
using NosSmooth.Packets.Server.Chat;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace WalkCommands.Commands;

/// <summary>
/// Represents command group for walking.
/// </summary>
public class WalkCommands : CommandGroup
{
    private readonly ManagedNostaleClient _client;
    private readonly PetManagerList _petManagerList;
    private readonly WalkManager _walkManager;
    private readonly FeedbackService _feedbackService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkCommands"/> class.
    /// </summary>
    /// <param name="client">The nostale client.</param>
    /// <param name="petManagerList">The pet manager list.</param>
    /// <param name="walkManager">The walk manager.</param>
    /// <param name="feedbackService">The feedback service.</param>
    public WalkCommands(ManagedNostaleClient client, PetManagerList petManagerList, WalkManager walkManager, FeedbackService feedbackService)
    {
        _client = client;
        _petManagerList = petManagerList;
        _walkManager = walkManager;
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Attempts to walk the character to the specified lcoation.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="isCancellable">Whether the user can cancel the operation.</param>
    /// <param name="petSelectors">The pet selectors indices.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Command("walk")]
    public async Task<Result> HandleWalkToAsync
    (
        short x,
        short y,
        bool isCancellable = true,
        [Option('p', "pet")]
        params int[] petSelectors
    )
    {
        var receiveResult = await _client.ReceivePacketAsync
        (
            new SayPacket(EntityType.Map, 1, SayColor.Red, $"Going to walk to {x} {y}."),
            CancellationToken
        );

        if (!receiveResult.IsSuccess)
        {
            return receiveResult;
        }

        var pets = petSelectors
            .Select(i => _petManagerList[i].Entity.Id)
            .Select(id => (id, x, y));

        var command = new WalkCommand
        (
            x,
            y,
            2,
            pets.ToArray(),
            AllowUserCancel: isCancellable
        );
        var walkResult = await _client.SendCommandAsync(command, CancellationToken);
        if (!walkResult.IsSuccess)
        {
            await _feedbackService.SendErrorMessageAsync
                ($"Could not finish walking. {walkResult.ToFullString()}", CancellationToken);
            await _client.ReceivePacketAsync
            (
                new SayPacket(EntityType.Map, 1, SayColor.Red, "Could not finish walking."),
                CancellationToken
            );
            return walkResult;
        }

        return await _client.ReceivePacketAsync
        (
            new SayPacket(EntityType.Map, 1, SayColor.Red, "Walk has finished successfully."),
            CancellationToken
        );
    }

    /// <summary>
    /// Attempts to walk the character to the specified lcoation using path finding.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="isCancellable">Whether the user can cancel the operation.</param>
    /// <param name="petSelectors">The pet selectors indices.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    [Command("pwalk")]
    public async Task<Result> HandlePathfindingWalkToAsync
    (
        short x,
        short y,
        bool isCancellable = true,
        [Option('p', "pet")]
        params int[] petSelectors
    )
    {
        var receiveResult = await _client.ReceivePacketAsync
        (
            new SayPacket(EntityType.Map, 1, SayColor.Red, $"Going to walk to {x} {y}."),
            CancellationToken
        );

        if (!receiveResult.IsSuccess)
        {
            return receiveResult;
        }
        
        var pets = petSelectors
            .Select(i => _petManagerList[i].Entity.Id)
            .Select(id => (id, x, y));

        var tasks = new List<Task<Result>>();

        tasks.Add(_walkManager.PlayerGoToAsync
        (
            x,
            y,
            isCancellable,
            CancellationToken
        ));

        foreach (var pet in pets)
        {
            tasks.Add(_walkManager.MateWalkToAsync(pet.id, pet.x, pet.y, isCancellable, CancellationToken));
        }

        var results = await Task.WhenAll(tasks);
        var errorfulResults = results.Where(x => !x.IsSuccess).OfType<IResult>().ToArray();
        if (errorfulResults.Length > 0)
        {
            await _client.ReceivePacketAsync
            (
                new SayPacket(EntityType.Map, 1, SayColor.Red, "Could not finish walking."),
                CancellationToken
            );

            var result = errorfulResults.Length switch
            {
                1 => (Result)errorfulResults[0],
                _ => new AggregateError(errorfulResults)
            };
            await _feedbackService.SendErrorMessageAsync
                ($"Could not finish walking. {result.ToFullString()}", CancellationToken);
            
            return result;
        }

        return await _client.ReceivePacketAsync
        (
            new SayPacket(EntityType.Map, 1, SayColor.Red, "Walk has finished successfully."),
            CancellationToken
        );
    }
}