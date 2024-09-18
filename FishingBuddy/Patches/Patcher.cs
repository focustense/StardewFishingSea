using HarmonyLib;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace FishingBuddy.Patches;

internal static class Patcher
{
    public static void ApplyAll(string modId)
    {
        var harmony = new Harmony(modId);
        ApplyItemQueryFixes(harmony);
        ApplyFishRandomizationPatches(harmony);
        ApplyCharacterPatches(harmony);
    }

    private static void ApplyFishRandomizationPatches(Harmony harmony)
    {
        // GetFishFromLocation data is public, but the overload containing the real
        // implementation is internal.
        var getFishFromLocationDataMethod = AccessTools.Method(
            typeof(GameLocation),
            nameof(GameLocation.GetFishFromLocationData),
            [
                typeof(string), // locationName
                typeof(Vector2), // bobberTile
                typeof(int), // waterDepth
                typeof(Farmer), // player
                typeof(bool), // isTutorialCatch
                typeof(bool), // isInherited
                typeof(GameLocation),
                typeof(ItemQueryContext),
            ]
        );
        var allGameRandomRefsTranspilerMethod = new HarmonyMethod(
            typeof(LocationFishPatches),
            nameof(LocationFishPatches.AllGameRandomRefsTranspiler)
        );
        harmony.Patch(getFishFromLocationDataMethod, transpiler: allGameRandomRefsTranspilerMethod);
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), "CheckGenericFishRequirements"),
            transpiler: allGameRandomRefsTranspilerMethod
        );
        var orderings = LocationFishPatches.GetOrderByMethods(getFishFromLocationDataMethod);
        foreach (var orderMethod in orderings)
        {
            harmony.Patch(orderMethod, transpiler: allGameRandomRefsTranspilerMethod);
        }
        harmony.Patch(
            AccessTools.Method(
                typeof(MineShaft),
                nameof(MineShaft.getFish),
                [
                    typeof(float), // millisecondsAfterNibble
                    typeof(string), // bait
                    typeof(int), // waterDepth
                    typeof(Farmer),
                    typeof(double), // baitPotency
                    typeof(Vector2), // bobberTile
                    typeof(string), // locationName
                ]
            ),
            transpiler: allGameRandomRefsTranspilerMethod
        );
    }

    private static void ApplyItemQueryFixes(Harmony harmony)
    {
        // ItemQueryResolver.TryResolve has an apparent bug in one of its overloads that goes
        // directly to Game1.random instead of using context.Random which would point to the
        // replayable instance. Patch it to use the context.
        var itemQueryTryResolveMethod = AccessTools.Method(
            typeof(ItemQueryResolver),
            nameof(ItemQueryResolver.TryResolve),
            [
                typeof(string), // query
                typeof(ItemQueryContext),
                typeof(ItemQuerySearchMode),
                typeof(string), // perItemCondition
                typeof(int?), // maxItems
                typeof(bool), // avoidRepeat
                typeof(HashSet<string>), // avoidItemIds
                typeof(Action<string, string>), // logError
            ]
        );
        harmony.Patch(
            itemQueryTryResolveMethod,
            transpiler: new(typeof(ItemQueryPatches), nameof(ItemQueryPatches.TryResolveTranspiler))
        );
    }

    private static void ApplyCharacterPatches(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.PropertyGetter(typeof(Character), nameof(Character.speed)),
            postfix: new(typeof(CharacterPatches), nameof(CharacterPatches.GetSpeed_Postfix))
        );
        harmony.Patch(
            AccessTools.Method(typeof(Character), nameof(Character.MovePosition)),
            prefix: new(typeof(CharacterPatches), nameof(CharacterPatches.MovePosition_Prefix)),
            postfix: new(typeof(CharacterPatches), nameof(CharacterPatches.MovePosition_Postfix)),
            transpiler: new(
                typeof(CharacterPatches),
                nameof(CharacterPatches.MovePosition_Transpiler)
            )
        );
        harmony.Patch(
            AccessTools.Method(typeof(PathFindController), nameof(PathFindController.update)),
            transpiler: new(
                typeof(PathfindingPatches),
                nameof(PathfindingPatches.PathFindController_Update_Transpiler)
            )
        );
    }
}
