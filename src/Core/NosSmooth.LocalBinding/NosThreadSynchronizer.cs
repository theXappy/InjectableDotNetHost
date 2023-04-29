//
//  NosThreadSynchronizer.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Options;
using Remora.Results;

namespace NosSmooth.LocalBinding;

/// <summary>
/// Synchronizes with NosTale thread using a periodic function.
/// </summary>
public class NosThreadSynchronizer
{
    private readonly Optional<IPeriodicHook> _periodicHook;
    private readonly ILogger<NosThreadSynchronizer> _logger;
    private readonly NosThreadSynchronizerOptions _options;
    private readonly ConcurrentQueue<SyncOperation> _queuedOperations;
    private Thread? _nostaleThread;

    /// <summary>
    /// Initializes a new instance of the <see cref="NosThreadSynchronizer"/> class.
    /// </summary>
    /// <param name="periodicHook">The periodic hook.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public NosThreadSynchronizer
    (
        Optional<IPeriodicHook> periodicHook,
        ILogger<NosThreadSynchronizer> logger,
        IOptions<NosThreadSynchronizerOptions> options
    )
    {
        _periodicHook = periodicHook;
        _logger = logger;
        _options = options.Value;
        _queuedOperations = new ConcurrentQueue<SyncOperation>();
    }

    /// <summary>
    /// Gets whether the current thread is a NosTale thread.
    /// </summary>
    public bool IsSynchronized => _nostaleThread == Thread.CurrentThread;

    /// <summary>
    /// Start the synchronizer operation.
    /// </summary>
    /// <returns>The result, successful if periodic hook is present.</returns>
    public Result StartSynchronizer()
    {
        return _periodicHook.TryDo(h => h.Called += PeriodicCall)
            ? Result.FromSuccess()
            : new NeededModulesNotInitializedError("Could not start synchronizer, because the periodic hook is not present", IHookManager.PeriodicName);
    }

    /// <summary>
    /// Stop the synchronizer operation.
    /// </summary>
    public void StopSynchronizer()
    {
        _periodicHook.TryDo(h => h.Called -= PeriodicCall);
    }

    private void PeriodicCall(object? owner, System.EventArgs eventArgs)
    {
        _nostaleThread = Thread.CurrentThread;
        var tasks = _options.MaxTasksPerIteration;

        while (tasks-- > 0 && _queuedOperations.TryDequeue(out var operation))
        {
            ExecuteOperation(operation);
        }
    }

    private void ExecuteOperation(SyncOperation operation)
    {
        try
        {
            var result = operation.Action();
            operation.Result = result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Synchronizer obtained an exception");
            operation.Result = (Result)e;
        }

        if (operation.CancellationTokenSource is not null)
        {
            try
            {
                operation.CancellationTokenSource.Cancel();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }

    /// <summary>
    /// Enqueue the given operation to execute on next frame.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="executeIfSynchronized">Whether to execute the operation instantly in case we are on the NosTale thread.</param>
    public void EnqueueOperation(Action action, bool executeIfSynchronized = true)
    {
        if (executeIfSynchronized && IsSynchronized)
        { // we are synchronized, no need to wait.
            action();
            return;
        }

        _queuedOperations.Enqueue
        (
            new SyncOperation
            (
                () =>
                {
                    action();
                    return Result.FromSuccess();
                },
                null
            )
        );
    }

    /// <summary>
    /// Synchronizes to NosTale thread, executes the given action and returns its result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The result of the action.</returns>
    public async Task<Result> SynchronizeAsync(Func<Result> action, CancellationToken ct = default)
    {
        return (Result)await CommonSynchronizeAsync(() => action(), ct);
    }

    /// <summary>
    /// Synchronizes to NosTale thread, executes the given action and returns its result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The result of the action.</returns>
    /// <typeparam name="T">The type of the result.</typeparam>
    public async Task<Result<T>> SynchronizeAsync<T>(Func<Result<T>> action, CancellationToken ct = default)
    {
        return (Result<T>)await CommonSynchronizeAsync(() => action(), ct);
    }

    private async Task<IResult> CommonSynchronizeAsync(Func<IResult> action, CancellationToken ct = default)
    {
        if (IsSynchronized)
        { // we are already synchronized, execute the action.
            try
            {
                return action();
            }
            catch (Exception e)
            {
                return (Result)e;
            }
        }

        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var syncOperation = new SyncOperation(action, linkedSource);
        _queuedOperations.Enqueue(syncOperation);

        try
        {
            await Task.Delay(Timeout.Infinite, linkedSource.Token);
        }
        catch (OperationCanceledException)
        {
            if (ct.IsCancellationRequested)
            { // Throw in case the top token was cancelled.
                throw;
            }
        }
        catch (Exception e)
        {
            return (Result)new ExceptionError(e);
        }

        return syncOperation.Result ?? Result.FromSuccess();
    }

    private record SyncOperation(Func<IResult> Action, CancellationTokenSource? CancellationTokenSource)
    {
        public IResult? Result { get; set; }
    }
}