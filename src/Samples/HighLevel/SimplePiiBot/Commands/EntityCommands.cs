//
//  EntityCommands.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.ChatCommands;
using NosSmooth.Extensions.Combat.Errors;
using NosSmooth.Game;
using NosSmooth.Game.Apis;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using NosSmooth.Packets.Enums.Chat;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace SimplePiiBot.Commands;

/// <summary>
/// Represents command group for combat commands.
/// </summary>
public class EntityCommands : CommandGroup
{
    private readonly Game _game;
    private readonly IHookManager _hookManager;
    private readonly NosThreadSynchronizer _synchronizer;
    private readonly SceneManager _sceneManager;
    private readonly FeedbackService _feedbackService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCommands"/> class.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="hookManager">The hook manager.</param>
    /// <param name="synchronizer">The thread synchronizer.</param>
    /// <param name="sceneManager">The scene manager.</param>
    /// <param name="feedbackService">The feedback service.</param>
    public EntityCommands
    (
        Game game,
        IHookManager hookManager,
        NosThreadSynchronizer synchronizer,
        SceneManager sceneManager,
        FeedbackService feedbackService
    )
    {
        _game = game;
        _hookManager = hookManager;
        _synchronizer = synchronizer;
        _sceneManager = sceneManager;
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Show close entities.
    /// </summary>
    /// <param name="range">The range to look.</param>
    /// <returns>A task that may or may not have succeeded.</returns>
    [Command("close")]
    public async Task<Result> HandleCloseEntitiesAsync(int range = 10)
    {
        var map = _game.CurrentMap;
        var character = _game.Character;
        if (map is null)
        {
            return new MapNotInitializedError();
        }

        if (character is null)
        {
            return new CharacterNotInitializedError();
        }

        var position = character.Position;
        if (position is null)
        {
            return new CharacterNotInitializedError("Position");
        }

        var entities = map.Entities
            .GetEntities()
            .Where(x => x.Position?.DistanceSquared(position.Value) <= range * range)
            .ToList();

        if (entities.Count == 0)
        {
            await _feedbackService.SendInfoMessageAsync("No entity found.", CancellationToken);
        }

        foreach (var entity in entities)
        {
            await _feedbackService.SendMessageAsync($"Found entity: {entity.Id}", SayColor.Yellow, CancellationToken);
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Focus the given entity.
    /// </summary>
    /// <param name="entityId">The entity id to focus.</param>
    /// <returns>A task that may or may not have succeeded.</returns>
    [Command("focus")]
    public async Task<Result> HandleFocusAsync(int entityId)
    {
        var entityResult = _sceneManager.FindEntity(entityId);
        if (!entityResult.IsDefined(out var entity))
        {
            return Result.FromError(entityResult);
        }

        return await _synchronizer.SynchronizeAsync
        (
            () => _hookManager.EntityFocus.MapResult
            (
                focus => focus.WrapperFunction.MapResult(wrapper => wrapper(entity))
            ),
            CancellationToken
        );
    }

    /// <summary>
    /// Follow the given entity.
    /// </summary>
    /// <param name="entityId">The entity id to follow.</param>
    /// <returns>A task that may or may not have succeeded.</returns>
    [Command("follow")]
    public async Task<Result> HandleFollowAsync(int entityId)
    {
        var entityResult = _sceneManager.FindEntity(entityId);
        if (!entityResult.IsDefined(out var entity))
        {
            return Result.FromError(entityResult);
        }

        return await _synchronizer.SynchronizeAsync
        (
            () => _hookManager.EntityFollow.MapResult
            (
                follow => follow.WrapperFunction.MapResult(wrapper => wrapper(entity))
            ),
            CancellationToken
        );
    }

    /// <summary>
    /// Stop following an entity.
    /// </summary>
    /// <returns>A task that may or may not have succeeded.</returns>
    [Command("unfollow")]
    public async Task<Result> HandleUnfollowAsync()
    {
        return await _synchronizer.SynchronizeAsync
        (
            () => _hookManager.EntityUnfollow.MapResult
            (
                unfollow => unfollow.WrapperFunction.MapResult(wrapper => wrapper())
            ),
            CancellationToken
        );
    }
}