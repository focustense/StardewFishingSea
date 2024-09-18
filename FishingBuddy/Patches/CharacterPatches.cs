using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace FishingBuddy.Patches;

internal static class CharacterPatches
{
    // HACK: Postfixing the getter introduces a possible isue with read-modify-write operations on
    // the speed, e.g. npc.speed += 2. If this happens, it will add to the multiplied speed, but the
    // setter will change the base speed.
    //
    // However, virtually all base-game logic that sets the speed, sets it to some constant or
    // predetermined value. The only instances where new speed is set a function of previous speed
    // occur in areas where fishing isn't possible. Modifying the setter to divide by the multiplier
    // would therefore be incorrect - much more so than leaving it alone.
    //
    // This should therefore be safe enough for vanilla but might have compatibility issues with
    // certain mods out there. Unfortunately, there isn't a foolproof way to determine whether some
    // arbitrary call to the setter is a result of an arithmetic operation, and the only alternative
    // would be to find every single call site that reads the speed (there are hundreds) and patch
    // them individually.
    public static void GetSpeed_Postfix(ref int __result)
    {
        __result *= PatchState.SpeedMultiplier;
    }

    public record MovePositionState(Rectangle BoundingBox, Point TargetTile);

    public static void MovePosition_Postfix(Character __instance, MovePositionState? __state)
    {
        if (__state is null)
        {
            return;
        }
        var previousTargetTile = __state.TargetTile;
        var targetTile = __instance.controller?.pathToEndPoint.Peek();
        if (targetTile != previousTargetTile)
        {
            // Can't detect overshoot if the tile has changed (and means pathfinding probably
            // updated anyway).
            return;
        }
        var previousBounds = __state.BoundingBox;
        var currentBounds = __instance.GetBoundingBox();
        var tileBounds = new Rectangle(targetTile.Value.X * 64, targetTile.Value.Y * 64, 64, 64);
        tileBounds.Inflate(-2, 0);
        if (tileBounds.Contains(currentBounds))
        {
            // Don't try to correct if character actually hit the target.
            return;
        }

        var width = currentBounds.Width;
        var height = currentBounds.Height;
        // Correct overshoot if moving left.
        if (
            previousBounds.Left > tileBounds.Left
            && currentBounds.Left < tileBounds.Left
            && WouldTargetContain(tileBounds.Left, currentBounds.Top)
        )
        {
            var dx = tileBounds.Left - currentBounds.Left;
            __instance.position.X += dx;
            LogCorrection(__instance, dx, 0);
        }
        // Correct overshoot if moving right.
        else if (
            previousBounds.Right < tileBounds.Right
            && currentBounds.Right > tileBounds.Right
            && WouldTargetContain(tileBounds.Right - width, currentBounds.Top)
        )
        {
            var dx = tileBounds.Right - currentBounds.Right;
            __instance.position.X += dx;
            LogCorrection(__instance, dx, 0);
        }
        // Correct overshoot if moving up.
        if (
            previousBounds.Top > tileBounds.Top
            && currentBounds.Top < tileBounds.Top
            && WouldTargetContain(currentBounds.Left, tileBounds.Top)
        )
        {
            var dy = tileBounds.Top - currentBounds.Top;
            __instance.position.Y += dy;
            LogCorrection(__instance, 0, dy);
        }
        // Correct overshoot if moving down.
        else if (
            previousBounds.Bottom < tileBounds.Bottom
            && currentBounds.Bottom > tileBounds.Bottom
            && WouldTargetContain(currentBounds.Left, tileBounds.Bottom - height)
        )
        {
            var dy = tileBounds.Bottom - currentBounds.Bottom;
            __instance.position.Y += dy;
            LogCorrection(__instance, 0, dy);
        }

        [Conditional("DEBUG_OVERSHOOT_CORRECTION")]
        static void LogCorrection(Character character, int dx, int dy)
        {
            var currentPosition = new Vector2(character.position.X, character.position.Y);
            var previousPosition = currentPosition - new Vector2(dx, dy);
            PatchState.Log(
                $"Corrected overshoot: {previousPosition} -> {currentPosition}",
                LogLevel.Debug
            );
        }

        bool WouldTargetContain(int x, int y)
        {
            return tileBounds.Contains(new Rectangle(x, y, width, height));
        }
    }

    public static void MovePosition_Prefix(Character __instance, out MovePositionState? __state)
    {
        if (__instance.controller is null || PatchState.SpeedMultiplier == 1)
        {
            __state = null;
            return;
        }
        var boundingBox = __instance.GetBoundingBox();
        var targetTile = __instance.controller.pathToEndPoint.Peek();
        __state = new(boundingBox, targetTile);
    }

    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Required by Harmony"
    )]
    public static IEnumerable<CodeInstruction> MovePosition_Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        var blockedIntervalField = AccessTools.Field(
            typeof(Character),
            nameof(Character.blockedInterval)
        );
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(
                new CodeMatch(OpCodes.Call, Members.TimeSpanGetMillisecondsMethod),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Stfld, blockedIntervalField)
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
