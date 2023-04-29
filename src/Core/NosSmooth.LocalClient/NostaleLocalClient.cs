//
//  NostaleLocalClient.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosSmooth.Core.Client;
using NosSmooth.Core.Commands;
using NosSmooth.Core.Commands.Control;
using NosSmooth.Core.Extensions;
using NosSmooth.Core.Packets;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using NosSmooth.Packets;
using NosSmooth.PacketSerializer;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using NosSmooth.PacketSerializer.Errors;
using Remora.Results;
using PacketEventArgs = NosSmooth.LocalBinding.EventArgs.PacketEventArgs;

namespace NosSmooth.LocalClient;

/// <summary>
/// The local nostale client.
/// </summary>
/// <remarks>
/// Client used for living in the same process as NostaleClientX.exe.
/// It hooks the send and receive packet methods.
/// </remarks>
public class NostaleLocalClient : BaseNostaleClient
{
    private readonly NosThreadSynchronizer _synchronizer;
    private readonly IHookManager _hookManager;
    private readonly ControlCommands _controlCommands;
    private readonly IPacketHandler _packetHandler;
    private readonly UserActionDetector _userActionDetector;
    private readonly ILogger _logger;
    private readonly IServiceProvider _provider;
    private readonly LocalClientOptions _options;
    private CancellationToken? _stopRequested;
    private IPacketInterceptor? _interceptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleLocalClient"/> class.
    /// </summary>
    /// <param name="synchronizer">The thread synchronizer.</param>
    /// <param name="hookManager">The hook manager.</param>
    /// <param name="controlCommands">The control commands.</param>
    /// <param name="commandProcessor">The command processor.</param>
    /// <param name="packetHandler">The packet handler.</param>
    /// <param name="userActionDetector">The user action detector.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options for the client.</param>
    /// <param name="provider">The dependency injection provider.</param>
    public NostaleLocalClient
    (
        NosThreadSynchronizer synchronizer,
        IHookManager hookManager,
        ControlCommands controlCommands,
        CommandProcessor commandProcessor,
        IPacketHandler packetHandler,
        UserActionDetector userActionDetector,
        ILogger<NostaleLocalClient> logger,
        IOptions<LocalClientOptions> options,
        IServiceProvider provider
    )
        : base(commandProcessor)
    {
        _options = options.Value;
        _synchronizer = synchronizer;
        _hookManager = hookManager;
        _controlCommands = controlCommands;
        _packetHandler = packetHandler;
        _userActionDetector = userActionDetector;
        _logger = logger;
        _provider = provider;
    }

    /// <inheritdoc />
    public override async Task<Result> RunAsync(CancellationToken stopRequested = default)
    {
        if (!_hookManager.IsHookLoaded<IPacketSendHook>() || !_hookManager.IsHookLoaded<IPacketReceiveHook>())
        {
            return new NeededModulesNotInitializedError
                ("Client cannot run", IHookManager.PacketSendName, IHookManager.PacketReceiveName);
        }

        _stopRequested = stopRequested;
        _logger.LogInformation("Starting local client");
        var synchronizerResult = _synchronizer.StartSynchronizer();
        if (!synchronizerResult.IsSuccess)
        {
            return synchronizerResult;
        }

        _hookManager.PacketSend.Get().Called += SendCallCallback;
        _hookManager.PacketReceive.Get().Called += ReceiveCallCallback;

        _hookManager.EntityFollow.TryDo(follow => follow.Called += FollowEntity);
        _hookManager.PlayerWalk.TryDo(walk => walk.Called += Walk);
        _hookManager.PetWalk.TryDo(walk => walk.Called += PetWalk);

        try
        {
            await Task.Delay(-1, stopRequested);
        }
        catch
        {
            // ignored
        }

        _hookManager.PacketSend.Get().Called -= SendCallCallback;
        _hookManager.PacketReceive.Get().Called -= ReceiveCallCallback;

        _hookManager.EntityFollow.TryDo(follow => follow.Called -= FollowEntity);
        _hookManager.PlayerWalk.TryDo(walk => walk.Called -= Walk);
        _hookManager.PetWalk.TryDo(walk => walk.Called -= PetWalk);

        // the hooks are not needed anymore.
        _hookManager.DisableAll();

        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public override async Task<Result> ReceivePacketAsync(string packetString, CancellationToken ct = default)
    {
        var result = _hookManager.PacketReceive.MapResult
        (
            receive => receive.WrapperFunction.MapResult
            (
                wrapperFunction =>
                {
                    _synchronizer.EnqueueOperation(() => wrapperFunction(packetString));
                    return Result.FromSuccess();
                }
            )
        );

        if (result.IsSuccess)
        {
            _logger.LogDebug($"Receiving client packet {packetString}");
            await ProcessPacketAsync(PacketSource.Server, packetString);
        }
        else
        {
            _logger.LogError("Could not receive packet");
            _logger.LogResultError(result);
        }

        return result;
    }

    /// <inheritdoc />
    public override async Task<Result> SendPacketAsync(string packetString, CancellationToken ct = default)
    {
        var result = _hookManager.PacketSend.MapResult
        (
            send => send.WrapperFunction.MapResult
            (
                wrapperFunction =>
                {
                    _synchronizer.EnqueueOperation(() => wrapperFunction(packetString));
                    return Result.FromSuccess();
                }
            )
        );

        if (result.IsSuccess)
        {
            _logger.LogDebug($"Sending client packet {packetString}");
            await ProcessPacketAsync(PacketSource.Server, packetString);
        }
        else
        {
            _logger.LogError("Could not send packet");
            _logger.LogResultError(result);
        }

        return result;
    }

    private void ReceiveCallCallback(object? owner, PacketEventArgs packetArgs)
    {
        bool accepted = true;
        var packet = packetArgs.Packet;
        if (_options.AllowIntercept)
        {
            if (_interceptor is null)
            {
                _interceptor = _provider.GetRequiredService<IPacketInterceptor>();
            }

            accepted = _interceptor.InterceptReceive(ref packet);
        }

        Task.Run(async () => await ProcessPacketAsync(PacketSource.Server, packet));

        if (!accepted)
        {
            packetArgs.Cancel = true;
        }
    }

    private void SendCallCallback(object? owner, PacketEventArgs packetArgs)
    {
        bool accepted = true;
        var packet = packetArgs.Packet;
        if (_options.AllowIntercept)
        {
            if (_interceptor is null)
            {
                _interceptor = _provider.GetRequiredService<IPacketInterceptor>();
            }

            accepted = _interceptor.InterceptSend(ref packet);
        }

        Task.Run(async () => await ProcessPacketAsync(PacketSource.Client, packet));

        if (!accepted)
        {
            packetArgs.Cancel = true;
        }
    }

    private void SendPacket(string packetString)
    {
        _synchronizer.EnqueueOperation
        (
            () => _hookManager.PacketSend.Get().WrapperFunction.Get()(packetString)
        );
        _logger.LogDebug($"Sending client packet {packetString}");
    }

    private async Task ProcessPacketAsync(PacketSource type, string packetString)
    {
        try
        {
            var result = await _packetHandler.HandlePacketAsync(this, type, packetString);

            if (!result.IsSuccess)
            {
                _logger.LogError("There was an error whilst handling packet {packetString}", packetString);
                _logger.LogResultError(result);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "The process packet threw an exception");
        }
    }

    private void FollowEntity(object? owner, EntityEventArgs entityEventArgs)
    {
        if (entityEventArgs.Entity is not null)
        {
            Task.Run
            (
                async () => await _controlCommands.CancelAsync
                    (ControlCommandsFilter.UserCancellable, false, (CancellationToken)_stopRequested!)
            );
        }
    }

    private void PetWalk(object? owner, PetWalkEventArgs petWalkEventArgs)
    {
        if (!_userActionDetector.IsPetWalkUserOperation
            (petWalkEventArgs.PetManager, petWalkEventArgs.X, petWalkEventArgs.Y))
        { // do not cancel operations made by NosTale or bot
            return;
        }

        if (_controlCommands.AllowUserActions)
        {
            Task.Run
            (
                async () => await _controlCommands.CancelAsync
                    (ControlCommandsFilter.UserCancellable, false, (CancellationToken)_stopRequested!)
            );
        }
        else
        {
            petWalkEventArgs.Cancel = true;
        }
    }

    private void Walk(object? owner, WalkEventArgs walkEventArgs)
    {
        if (!_userActionDetector.IsWalkUserAction(walkEventArgs.X, walkEventArgs.Y))
        { // do not cancel operations made by NosTale or bot
            return;
        }

        if (_controlCommands.AllowUserActions)
        {
            Task.Run
            (
                async () => await _controlCommands.CancelAsync
                    (ControlCommandsFilter.UserCancellable, false, (CancellationToken)_stopRequested!)
            );
        }
        else
        {
            walkEventArgs.Cancel = true;
        }
    }
}