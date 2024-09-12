using StardewValley.Delegates;
using StardewValley.Tools;

namespace FishingBuddy.Data;

/// <summary>
/// Custom Game State Queries used by this mod.
/// </summary>
public static class Queries
{
    /// <summary>
    /// Game state query for when a player has a certain fishing tackle equipped.
    /// </summary>
    /// <inheritdoc cref="GameStateQueryDelegate"/>
    public static bool PLAYER_USING_TACKLE(string[] query, GameStateQueryContext context)
    {
        if (
            !ArgUtility.TryGet(query, 1, out var playerKey, out var error)
            || !ArgUtility.TryGet(query, 2, out var itemId, out error)
        )
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        return GameStateQuery.Helpers.WithPlayer(
            context.Player,
            playerKey,
            player =>
                player.CurrentTool is FishingRod rod
                && rod.GetTackleQualifiedItemIDs().Contains(itemId)
        );
    }
}
