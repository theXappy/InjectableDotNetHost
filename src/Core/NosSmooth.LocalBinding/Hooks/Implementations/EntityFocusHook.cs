//
//  EntityFocusHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Memory.Sources;
using Remora.Results;

namespace NosSmooth.LocalBinding.Hooks.Implementations;

/// <summary>
/// A hook of NetworkManager.EntityFocus.
/// </summary>
internal class EntityFocusHook : CancelableNostaleHook<IEntityFocusHook.EntityFocusDelegate,
    IEntityFocusHook.EntityFocusWrapperDelegate, EntityEventArgs>, IEntityFocusHook
{
    /// <summary>
    /// Create the packet send hook.
    /// </summary>
    /// <param name="bindingManager">The binding manager.</param>
    /// <param name="browserManager">The browser manager.</param>
    /// <param name="options">The options for the binding.</param>
    /// <returns>A packet send hook or an error.</returns>
    public static Result<EntityFocusHook> Create
        (NosBindingManager bindingManager, NosBrowserManager browserManager, HookOptions<IEntityFocusHook> options)
    {
        var hook = CreateHook
        (
            bindingManager,
            () => new EntityFocusHook(browserManager.Memory, browserManager.UnitManager),
            hook => hook.Detour,
            options
        );

        return hook;
    }

    private readonly Optional<UnitManager> _unitManager;
    private readonly IMemory _memory;

    private EntityFocusHook(IMemory memory, Optional<UnitManager> unitManager)
    {
        _memory = memory;
        _unitManager = unitManager;
    }

    /// <inheritdoc />
    public override string Name => IHookManager.EntityFocusName;

    /// <inheritdoc />
    public override Optional<IEntityFocusHook.EntityFocusWrapperDelegate> WrapperFunction
        => _unitManager.Map<IEntityFocusHook.EntityFocusWrapperDelegate>
            (unitManager => entity => OriginalFunction(unitManager.Address, entity?.Address ?? 0));

    /// <inheritdoc />
    protected override IEntityFocusHook.EntityFocusDelegate WrapWithCalling
        (IEntityFocusHook.EntityFocusDelegate function)
        => (unitManagerPtr, entityPtr) =>
        {
            CallingFromNosSmooth = true;
            var res = function(unitManagerPtr, entityPtr);
            CallingFromNosSmooth = false;
            return res;
        };

    private nuint Detour
    (
        nuint unitManagerPtr,
        nuint entityPtr
    )
    {
        var entity = new MapBaseObj(_memory, entityPtr);
        var entityArgs = new EntityEventArgs(entity);
        return HandleCall(entityArgs);
    }
}