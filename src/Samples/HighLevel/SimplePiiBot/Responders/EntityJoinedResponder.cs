//
//  EntityJoinedResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.Game.Apis;
using NosSmooth.Game.Apis.Safe;
using NosSmooth.Game.Data.Entities;
using NosSmooth.Game.Events.Core;
using NosSmooth.Game.Events.Entities;
using NosSmooth.Packets.Enums;
using NosSmooth.Packets.Enums.Players;
using Remora.Results;

namespace SimplePiiBot.Responders;

/// <summary>
/// Responds to entity joined map event.
/// </summary>
public class EntityJoinedResponder : IGameResponder<EntityJoinedMapEvent>
{
    private readonly Bot _bot;
    private readonly NostaleChatApi _chatApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityJoinedResponder"/> class.
    /// </summary>
    /// <param name="bot">The bot.</param>
    /// <param name="chatApi">The chat packet api.</param>
    public EntityJoinedResponder(Bot bot, NostaleChatApi chatApi)
    {
        _bot = bot;
        _chatApi = chatApi;
    }

    /// <inheritdoc />
    public async Task<Result> Respond(EntityJoinedMapEvent gameEvent, CancellationToken ct = default)
    {
        if (gameEvent.Entity is Player player)
        {
            if (player.Authority > AuthorityType.User)
            {
                var result = await _bot.StopAsync(ct);
                await _chatApi.ReceiveSystemMessageAsync("A GM has joined the map, stopping the bot.", ct: ct);

                return result;
            }
        }

        return Result.FromSuccess();
    }
}