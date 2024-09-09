using System.Runtime.CompilerServices;

namespace FishingBuddy;

/// <summary>
/// Normally-hardcoded rules specific to fish catches.
/// </summary>
/// <remarks>
/// Based on same compatibility philosophy as <see cref="GameRules"/>.
/// </remarks>
public static class CatchRules
{
    /// <summary>
    /// Creates the random instance used to perform seeded-fish random checks.
    /// </summary>
    /// <remarks>
    /// Normally implemented inline in <see cref="GameLocation.GetFishFromLocationData"/>.
    /// </remarks>
    /// <param name="fishCaught">Number of fish caught so far.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Random GetSeededFishRandom(uint fishCaught)
    {
        return Utility.CreateRandom(Game1.uniqueIDForThisGame, fishCaught * 859);
    }

    /// <summary>
    /// Checks if a bait object targets a specific fish.
    /// </summary>
    /// <param name="bait">The fish bait stack.</param>
    /// <returns>The qualified item ID of the targeted fish, or <c>null</c> if the bait is not
    /// targeted.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string? GetTargetFishId(SObject? bait)
    {
        return
            bait?.QualifiedItemId == "(O)SpecificBait"
            && bait.preservedParentSheetIndex.Value is not null
            ? "(O)" + bait.preservedParentSheetIndex.Value
            : null;
    }

    /// <summary>
    /// Checks whether or not a list of tackle IDs includes a Curiosity Lure.
    /// </summary>
    /// <remarks>
    /// Patch this if a mod adds some other tackle that is supposed to act the same way.
    /// </remarks>
    /// <param name="tackleIds">Qualified item IDs of tackle on the fishing rod.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool HasCuriosityLure(IEnumerable<string> tackleIds)
    {
        return tackleIds.Contains("(O)856");
    }
}
