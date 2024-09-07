using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.GameData.Locations;

namespace FishingBuddy;

internal static class LocationFishPatches
{
    private static readonly MethodInfo fishRandomGetMethod = AccessTools.PropertyGetter(
        typeof(FishRandom),
        nameof(FishRandom.Instance)
    );
    private static readonly FieldInfo gameRandomField = AccessTools.Field(
        typeof(Game1),
        nameof(Game1.random)
    );

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
            .MatchStartForward(new CodeMatch(OpCodes.Ldsfld, gameRandomField))
            .Repeat(matcher =>
            {
                // Don't use SetInstruction here because it will destroy the label.
                matcher.Opcode = OpCodes.Call;
                matcher.Operand = fishRandomGetMethod;
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
