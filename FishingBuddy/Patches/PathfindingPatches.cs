using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Pathfinding;

namespace FishingBuddy.Patches;

internal static class PathfindingPatches
{
    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Required by Harmony"
    )]
    public static IEnumerable<CodeInstruction> PathFindController_Update_Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        var pausedTimerField = AccessTools.Field(
            typeof(PathFindController),
            nameof(PathFindController.pausedTimer)
        );
        var timerSinceLastCheckpointField = AccessTools.Field(
            typeof(PathFindController),
            nameof(PathFindController.timerSinceLastCheckPoint)
        );
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(
                new CodeMatch(OpCodes.Call, Members.TimeSpanGetMillisecondsMethod),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Stfld)
                {
                    operands = [pausedTimerField, timerSinceLastCheckpointField],
                }
            )
            .Repeat(match =>
                match
                    .Advance(2)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, Members.GetSpeedMultiplierMethod),
                        new CodeInstruction(OpCodes.Mul)
                    )
            )
            .InstructionEnumeration();
    }
}
