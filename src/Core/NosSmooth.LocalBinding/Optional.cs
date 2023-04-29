//
//  Optional.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using NosSmooth.LocalBinding.Errors;
using Remora.Results;

namespace NosSmooth.LocalBinding;

/// <summary>
/// An optional, used mainly for hooks and binding modules,
/// to make it possible to check whether a module is loaded
/// in runtime.
/// </summary>
/// <typeparam name="T">The type of underlying value.</typeparam>
public class Optional<T>
    where T : notnull
{
    /// <summary>
    /// An empty optional (no value present).
    /// </summary>
    public static readonly Optional<T> Empty = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{T}"/> class.
    /// </summary>
    /// <param name="value">The underlying value of the optional. Not present when null.</param>
    public Optional(T? value = default)
    {
        Value = value;
    }

    /// <summary>
    /// Gets whether the value is present.
    /// </summary>
    [MemberNotNullWhen(true, "Value")]
    public bool IsPresent => Value is not null;

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Tries to get the underlying value, if it's present.
    /// If it's not present, value won't be set.
    /// </summary>
    /// <param name="value">The underlying value.</param>
    /// <returns>Whether the value is present.</returns>
    public bool TryGet([NotNullWhen(true)] out T? value)
    {
        value = Value;
        return IsPresent;
    }

    /// <summary>
    /// Tries to execute an action on the value, if it exists.
    /// </summary>
    /// <remarks>
    /// Does nothing, if the value does not exist.
    /// </remarks>
    /// <param name="action">The action to execute.</param>
    /// <returns>Whether the value is present.</returns>
    public bool TryDo(Action<T> action)
    {
        if (IsPresent)
        {
            action(Value);
        }

        return IsPresent;
    }

    /// <summary>
    /// Gets something from the underlying value,
    /// exposing it as optional that is empty,
    /// in case this optional is empty as well.
    /// </summary>
    /// <param name="get">The function to obtain something from the value with.</param>
    /// <typeparam name="TU">The return type.</typeparam>
    /// <returns>An optional, present if this optional's value is present.</returns>
    public Optional<TU> Map<TU>(Func<T, TU> get)
        where TU : notnull
    {
        if (IsPresent)
        {
            return get(Value);
        }

        return Optional<TU>.Empty;
    }

    /// <summary>
    /// Gets something from the underlying value like <see cref="Map{TU}"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="OptionalNotPresentError"/> in case this value is not present
    /// instead of returning an optional.
    /// </remarks>
    /// <param name="get">The function to obtain something from the value with.</param>
    /// <typeparam name="TU">The return type.</typeparam>
    /// <returns>A result, successful in case this optional's value is present.</returns>
    public Result<TU> MapResult<TU>(Func<T, TU> get)
        where TU : notnull
    {
        if (IsPresent)
        {
            return OptionalUtilities.TryGet(() => get(Value));
        }

        return new OptionalNotPresentError(typeof(T).Name);
    }

    /// <summary>
    /// Does something on the underlying value like <see cref="TryDo"/>, but returns a result
    /// in case the value is not present.
    /// </summary>
    /// <param name="get">The function to execute on the value.</param>
    /// <typeparam name="TU">The return type.</typeparam>
    /// <returns>A result, successful in case this optional's value is present.</returns>
    public Result MapResult(Action<T> get)
    {
        if (IsPresent)
        {
            return OptionalUtilities.Try(() => get(Value));
        }

        return new OptionalNotPresentError(typeof(T).Name);
    }

    /// <summary>
    /// Gets something from the underlying value like <see cref="Map{TU}"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="OptionalNotPresentError"/> in case this value is not present
    /// instead of returning an optional.
    ///
    /// The get function returns a result that will be returned if this optional is present.
    /// </remarks>
    /// <param name="get">The function to obtain something from the value with.</param>
    /// <typeparam name="TU">The return type.</typeparam>
    /// <returns>A result from the function, <see cref="OptionalNotPresentError"/> in case this optional is not present.</returns>
    public Result<TU> MapResult<TU>(Func<T, Result<TU>> get)
        where TU : notnull
    {
        if (IsPresent)
        {
            return get(Value);
        }

        return new OptionalNotPresentError(typeof(T).Name);
    }

    /// <summary>
    /// Does something on the underlying value like <see cref="TryDo"/>, but returns a result
    /// in case the value is not present.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="OptionalNotPresentError"/> in case this value is not present
    /// instead of returning an optional.
    ///
    /// The get function returns a result that will be returned if this optional is present.
    /// </remarks>
    /// <param name="get">The function to obtain something from the value with.</param>
    /// <typeparam name="TU">The return type.</typeparam>
    /// <returns>A result from the function, <see cref="OptionalNotPresentError"/> in case this optional is not present.</returns>
    public Result MapResult(Func<T, Result> get)
    {
        if (IsPresent)
        {
            return get(Value);
        }

        return new OptionalNotPresentError(typeof(T).Name);
    }

    /// <summary>
    /// Forcefully gets the underlying value.
    /// If it's not present, <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    /// <remarks>
    /// Try to use other methods that return results where possible as they are easier to handle.
    /// </remarks>
    /// <returns>The underlying value.</returns>
    /// <exception cref="InvalidOperationException">Thrown in case the value is not present.</exception>
    public T Get()
    {
        if (!IsPresent)
        {
            throw new InvalidOperationException
            (
                $"Could not get {nameof(T)}. Did you forget to call initialization or was there an error?"
            );
        }

        return Value;
    }

    /// <summary>
    /// Cast a value to optional.
    /// </summary>
    /// <param name="val">The value to cast.</param>
    /// <returns>The created optional.</returns>
    public static implicit operator Optional<T>(T val)
    {
        return new Optional<T>(val);
    }
}