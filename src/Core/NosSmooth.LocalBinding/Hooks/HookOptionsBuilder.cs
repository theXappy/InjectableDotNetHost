//
//  HookOptionsBuilder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// Builder for <see cref="HookOptions"/>.
/// </summary>
/// <typeparam name="T">The type of hook options.</typeparam>
public class HookOptionsBuilder<T>
{
    private string _name;
    private bool _hook;
    private string _pattern;
    private int _offset;

    /// <summary>
    /// Initializes a new instance of the <see cref="HookOptionsBuilder{T}"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    internal HookOptionsBuilder(HookOptions<T> options)
    {
        _name = options.Name;
        _hook = options.Hook;
        _pattern = options.MemoryPattern;
        _offset = options.Offset;
    }

    /// <summary>
    /// Configure whether to hook this function.
    /// Default true.
    /// </summary>
    /// <param name="hook">Whether to hook the function.</param>
    /// <returns>This builder.</returns>
    public HookOptionsBuilder<T> Hook(bool hook = true)
    {
        _hook = hook;
        return this;
    }

    /// <summary>
    /// Configure the memory pattern.
    /// Use ?? for any bytes.
    /// </summary>
    /// <param name="pattern">The memory pattern.</param>
    /// <returns>This builder.</returns>
    public HookOptionsBuilder<T> MemoryPattern(string pattern)
    {
        _pattern = pattern;
        return this;
    }

    /// <summary>
    /// Configure the offset from the pattern.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <returns>This builder.</returns>
    public HookOptionsBuilder<T> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    /// <summary>
    /// Create hook options from this builder.
    /// </summary>
    /// <returns>The options.</returns>
    internal HookOptions<T> Build()
        => new HookOptions<T>(_name, _hook, _pattern, _offset);
}