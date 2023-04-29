//
//  OptionalUtilities.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.LocalBinding;

/// <summary>
/// A utilities that work with members that may not be present or may throw an exception.
/// </summary>
public static class OptionalUtilities
{
    /// <summary>
    /// Tries to get value from the function, capturing an exception into Result.
    /// </summary>
    /// <param name="get">The function to obtain the value from.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The value, or exception error if an exception has been thrown.</returns>
    public static Result<T> TryGet<T>(Func<T> get)
    {
        try
        {
            return get();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Tries to get value from the function, capturing an exception into Result.
    /// </summary>
    /// <param name="get">The function to obtain the value from.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The value, or exception error if an exception has been thrown.</returns>
    public static Result<T> TryIGet<T>(Func<Result<T>> get)
    {
        try
        {
            return get();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Tries to execute an action.
    /// </summary>
    /// <param name="get">The action.</param>
    /// <returns>A result, successful if no exception has been thrown.</returns>
    public static Result Try(Action get)
    {
        try
        {
            get();
            return Result.FromSuccess();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}