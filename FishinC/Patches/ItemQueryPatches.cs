﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Internal;

namespace FishinC.Patches;

internal static class ItemQueryPatches
{
    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Required by Harmony"
    )]
    public static IEnumerable<CodeInstruction> TryResolveTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        // Original: var result = Game1.random.ChooseFrom(...)
        // Patched:  var result = GetContextOrDefaultRandom(context).ChooseFrom(...)
        var getContextOrDefaultRandomMethod = AccessTools.Method(
            typeof(ItemQueryPatches),
            nameof(GetContextOrDefaultRandom)
        );
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(new CodeMatch(OpCodes.Ldsfld, Members.GameRandomField))
            .SetAndAdvance(OpCodes.Ldarg_1, null)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, getContextOrDefaultRandomMethod))
            .InstructionEnumeration();
    }

    private static Random GetContextOrDefaultRandom(ItemQueryContext? context)
    {
        return context?.Random ?? Game1.random;
    }
}
