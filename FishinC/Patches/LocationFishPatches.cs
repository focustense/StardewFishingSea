using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.GameData.Locations;

namespace FishinC.Patches;

internal static class LocationFishPatches
{
    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Required by Harmony"
    )]
    public static IEnumerable<CodeInstruction> AllGameRandomRefsTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        // Original: Game1.random (field)
        // Patched:  FishRandom.Instance (property)
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(new CodeMatch(OpCodes.Ldsfld, Members.GameRandomField))
            .Repeat(matcher =>
            {
                // Don't use SetInstruction here because it will destroy the label.
                matcher.Opcode = OpCodes.Call;
                matcher.Operand = Members.FishRandomGetMethod;
            })
            .InstructionEnumeration();
    }

    public static IEnumerable<MethodBase> GetOrderByMethods(
        MethodBase getFishFromLocationDataMethod
    )
    {
        var instructions = PatchProcessor.GetOriginalInstructions(getFishFromLocationDataMethod);
        var rankCtor = AccessTools.Constructor(
            typeof(Func<SpawnFishData, int>),
            [typeof(object), typeof(IntPtr)]
        );
        var rankMethods = new List<MethodInfo>();
        new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(OpCodes.Newobj, rankCtor))
            .Repeat(matcher =>
            {
                rankMethods.Add((MethodInfo)matcher.InstructionAt(-1).operand);
                matcher.Advance(1);
            });
        return rankMethods;
    }
}
