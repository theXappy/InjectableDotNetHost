//
//  IPetWalkHook.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.LocalBinding.EventArgs;
using NosSmooth.LocalBinding.Structs;
using Reloaded.Hooks.Definitions.X86;

namespace NosSmooth.LocalBinding.Hooks;

/// <summary>
/// A hook of PetManager.Walk.
/// </summary>
public interface IPetWalkHook : INostaleHook<IPetWalkHook.PetWalkDelegate, IPetWalkHook.PetWalkWrapperDelegate, PetWalkEventArgs>
{
    /// <summary>
    /// NosTale walk function to hook.
    /// </summary>
    /// <param name="petManagerPtr">The pointer to a pet manager object.</param>
    /// <param name="position">The position to walk to. First 4 bits are x (most significant), next 4 bits are y.</param>
    /// <param name="unknown0">Unknown 1. TODO.</param>
    /// <param name="unknown1">Unknown 2. TODO.</param>
    /// <param name="unknown2">Unknown 3. TODO.</param>
    /// <returns>1 to proceed to NosTale function, 0 to block the call.</returns>
    [Function
    (
        new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.edx, FunctionAttribute.Register.ecx },
        FunctionAttribute.Register.eax,
        FunctionAttribute.StackCleanup.Callee,
        new[] { FunctionAttribute.Register.ebx, FunctionAttribute.Register.esi, FunctionAttribute.Register.edi, FunctionAttribute.Register.ebp }
    )]
    public delegate nuint PetWalkDelegate(nuint petManagerPtr, int position, short unknown0 = 0, int unknown1 = 1, int unknown2 = 1);

    /// <summary>
    /// Pet walk function.
    /// </summary>
    /// <param name="manager">The pet manager.</param>
    /// <param name="x">The x coordinate to walk to.</param>
    /// <param name="y">The y coordinate to walk to.</param>
    /// <returns>A bool signaling whether the operation was successful.</returns>
    public delegate bool PetWalkWrapperDelegate(PetManager manager, ushort x, ushort y);
}