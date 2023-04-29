//
//  UserActionDetector.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.SqlTypes;
using NosSmooth.LocalBinding.Hooks;
using NosSmooth.LocalBinding.Objects;
using NosSmooth.LocalBinding.Structs;
using Remora.Results;

namespace NosSmooth.LocalClient;

/// <summary>
/// Tries to determine whether NosTale function calls
/// were made by user.
/// </summary>
public class UserActionDetector
{
    private readonly SemaphoreSlim _semaphore;
    private bool _handlingDisabled;
    private (ushort X, ushort Y) _lastWalkPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserActionDetector"/> class.
    /// </summary>
    public UserActionDetector()
    {
        _semaphore = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Execute an action that makes sure walk won't be treated as a user action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <typeparam name="T">The return type.</typeparam>
    /// <returns>Return value of the action.</returns>
    public async Task<T> NotUserActionAsync<T>(Func<T> action, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        _handlingDisabled = true;
        var result = action();
        _handlingDisabled = false;
        _semaphore.Release();
        return result;
    }

    /// <summary>
    /// Execute an action that makes sure walk won't be treated as a user action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>Return value of the action.</returns>
    public async Task NotUserActionAsync(Action action, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        _handlingDisabled = true;
        action();
        _handlingDisabled = false;
        _semaphore.Release();
    }

    /// <summary>
    /// Execute an action that makes sure walk won't be treated as a user action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Result NotUserAction(Func<Result> action)
    {
        _semaphore.Wait();
        _handlingDisabled = true;
        var result = action();
        _handlingDisabled = false;
        _semaphore.Release();
        return result;
    }

    /// <summary>
    /// Execute an action that makes sure walk won't be treated as a user action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <typeparam name="T">The return type.</typeparam>
    /// <returns>Return value of the action.</returns>
    public T NotUserAction<T>(Func<T> action)
    {
        _semaphore.Wait();
        _handlingDisabled = true;
        var result = action();
        _handlingDisabled = false;
        _semaphore.Release();
        return result;
    }

    /// <summary>
    /// Execute walk action and make sure walk won't be treated as a user action.
    /// </summary>
    /// <param name="walkHook">The walk hook.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The result of the walk.</returns>
    public Result<bool> NotUserWalk(IPlayerWalkHook walkHook, short x, short y)
        => NotUserAction
        (
            () =>
            {
                _lastWalkPosition = ((ushort)x, (ushort)y);
                return walkHook.WrapperFunction.MapResult(func => func((ushort)x, (ushort)y));
            }
        );

    /// <summary>
    /// Execute walk action and make sure walk won't be treated as a user action.
    /// </summary>
    /// <param name="walkHook">The walk hook.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The result of the walk.</returns>
    public async Task<Result<bool>> NotUserWalkAsync(IPlayerWalkHook walkHook, short x, short y, CancellationToken ct = default)
        => await NotUserActionAsync
        (
            () =>
            {
                _lastWalkPosition = ((ushort)x, (ushort)y);
                return walkHook.WrapperFunction.MapResult(func => func((ushort)x, (ushort)y));
            },
            ct
        );

    /// <summary>
    /// Checks whether the given Walk call
    /// is a user action or not (either bot or NosTale action).
    /// </summary>
    /// <param name="x">The x coordinate of Walk call.</param>
    /// <param name="y">The y coordinate of Walk call.</param>
    /// <returns>Whether the action is a user action.</returns>
    public bool IsWalkUserAction(ushort x, ushort y)
    {
        if (_handlingDisabled)
        {
            _lastWalkPosition = (x, y);
            return false;
        }

        if (_lastWalkPosition.X == x && _lastWalkPosition.Y == y)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks whether the given PetWalk call
    /// is a user action or not (either bot or NosTale action).
    /// </summary>
    /// <param name="petManager">The pet manager.</param>
    /// <param name="x">The x coordinate of PetWalk call.</param>
    /// <param name="y">The y coordinate of PetWalk call.</param>
    /// <returns>Whether the action is a user action.</returns>
    public bool IsPetWalkUserOperation(PetManager petManager, ushort x, ushort y)
    {
        return false; // TODO: implement user action detection for petwalk
        /*if (_handlingDisabled)
        {
            return false;
        }

        return true;*/
    }
}