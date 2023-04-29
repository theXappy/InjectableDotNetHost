//
//  Bot.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using NosSmooth.Core.Extensions;
using NosSmooth.Core.Stateful;
using NosSmooth.Extensions.Combat;
using NosSmooth.Extensions.Combat.Policies;
using NosSmooth.Extensions.Combat.Techniques;
using NosSmooth.Extensions.Pathfinding;
using NosSmooth.Game;
using NosSmooth.Game.Apis;
using NosSmooth.Game.Apis.Safe;
using NosSmooth.Game.Data.Characters;
using NosSmooth.Game.Data.Entities;
using NosSmooth.Game.Data.Info;
using NosSmooth.Game.Data.Maps;
using Remora.Results;

namespace SimplePiiBot;

/// <summary>
/// The pii bot.
/// </summary>
public class Bot : IStatefulEntity
{
    private static readonly long[] PiiPods = { 45, 46, 47, 48, 49, 50, 51, 52, 53 };
    private static readonly long[] Piis = { 36, 37, 38, 39, 40, 41, 42, 43, 44 };
    private static readonly long RangeSquared = 15 * 15;
    private static readonly long MaxPiiCount = 15;

    private readonly NostaleChatApi _chatPacketApi;
    private readonly NostaleSkillsApi _skillsApi;
    private readonly CombatManager _combatManager;
    private readonly Game _game;
    private readonly WalkManager _walkManager;
    private readonly ILogger<Bot> _logger;
    private CancellationTokenSource? _startCt;

    /// <summary>
    /// Initializes a new instance of the <see cref="Bot"/> class.
    /// </summary>
    /// <param name="chatPacketApi">The chat packet api.</param>
    /// <param name="skillsApi">The skills api.</param>
    /// <param name="combatManager">The combat manager.</param>
    /// <param name="game">The game.</param>
    /// <param name="walkManager">The walk manager.</param>
    /// <param name="logger">The logger.</param>
    public Bot
    (
        NostaleChatApi chatPacketApi,
        NostaleSkillsApi skillsApi,
        CombatManager combatManager,
        Game game,
        WalkManager walkManager,
        ILogger<Bot> logger
    )
    {
        _chatPacketApi = chatPacketApi;
        _skillsApi = skillsApi;
        _combatManager = combatManager;
        _game = game;
        _walkManager = walkManager;
        _logger = logger;
    }

    /// <summary>
    /// Start the bot.
    /// </summary>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>A result that may or may not succeed.</returns>
    public async Task<Result> StartAsync(CancellationToken ct = default)
    {
        if (_startCt is not null)
        {
            return new GenericError("The bot is already running.");
        }

        Task.Run
        (
            async () =>
            {
                try
                {
                    await Run(ct);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "The bot threw an exception");
                }
            }
        );

        return Result.FromSuccess();
    }

    private async Task Run(CancellationToken ct)
    {
        await _chatPacketApi.ReceiveSystemMessageAsync("Starting the bot.", ct: ct);
        _startCt = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ct = _startCt.Token;
        while (!ct.IsCancellationRequested)
        {
            var map = _game.CurrentMap;
            if (map is null)
            {
                await _chatPacketApi.ReceiveSystemMessageAsync("The map is null, quitting. Change the map.", ct: ct);
                await StopAsync();
                return;
            }

            var character = _game.Character;
            if (character is null || character.Position is null)
            {
                await _chatPacketApi.ReceiveSystemMessageAsync
                    ("The character is null, quitting. Change the map.", ct: ct);
                await StopAsync();
                return;
            }

            var entity = ChooseNextEntity(map, character, character.Position.Value);
            if (entity is null)
            {
                await _chatPacketApi.ReceiveSystemMessageAsync
                    ("There are no piis in range.", ct: ct);
                await StopAsync();
                return;
            }

            var combatResult = await _combatManager.EnterCombatAsync
            (
                new SimpleAttackTechnique
                (
                    entity.Id,
                    _skillsApi,
                    _walkManager,
                    new SkillSelector(Piis.Contains(entity.VNum)),
                    new UseItemPolicy
                    (
                        false,
                        0,
                        0,
                        Array.Empty<int>(),
                        Array.Empty<int>()
                    )
                ),
                ct
            );

            if (!combatResult.IsSuccess)
            {
                _logger.LogResultError(combatResult);
                await StopAsync();
                return;
            }
        }
    }

    private Monster? ChooseNextEntity(Map map, Character character, Position characterPosition)
    {
        var piisCount = map.Entities
            .GetEntities()
            .Where(x => x.Position?.DistanceSquared(characterPosition) <= RangeSquared)
            .OfType<Monster>()
            .Count(x => Piis.Contains(x.VNum) && x.Hp?.Percentage > 0);

        var choosingList = PiiPods;
        if (piisCount >= MaxPiiCount)
        { // max count of piis reached, choose pii instead of a pad
            choosingList = Piis;
        }

        var piiOrPod = GetEntity(choosingList, map, characterPosition);

        if (piiOrPod is null && piisCount != 0)
        {
            piiOrPod = GetEntity(Piis, map, characterPosition);
        }

        return piiOrPod;
    }

    private Monster? GetEntity(long[] choosingList, Map map, Position characterPosition)
    {
        return map.Entities.GetEntities()
            .OfType<Monster>()
            .Where(x => x.Hp?.Percentage > 0)
            .Where(x => x.Position?.DistanceSquared(characterPosition) <= RangeSquared)
            .Where(x => choosingList.Contains(x.VNum))
            .MinBy
            (
                x => x.Position is null
                    ? long.MaxValue
                    : characterPosition.DistanceSquared(x.Position.Value)
            );
    }

    /// <summary>
    /// Stop the bot.
    /// </summary>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>A result that may or may not succeed.</returns>
    public async Task<Result> StopAsync(CancellationToken ct = default)
    {
        var startCt = _startCt;
        try
        {
            var messageResult = await _chatPacketApi.ReceiveSystemMessageAsync("Stopping the bot.", ct: ct);
            if (startCt is not null)
            {
                startCt.Cancel();
                startCt.Dispose();
            }
            _startCt = null;

            return messageResult;
        }
        catch (Exception e)
        {
            // ignored
            return e;
        }
    }
}