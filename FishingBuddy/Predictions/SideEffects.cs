﻿using StardewValley.Locations;

namespace FishingBuddy.Predictions;

/// <summary>
/// Represents an unwanted side effect that can happen as a result of
/// <see cref="GameLocation.getFish"/> in the context of predictions.
/// </summary>
/// <remarks>
/// In most cases, <c>getFish</c> doesn't directly modify external state, as the name would imply.
/// However, there are a few cases where it does, which seem carefully lined up not to overlap with
/// locations where <c>getFish</c> would be called outside the minigame (e.g. anywhere a frenzy can
/// spawn). This interface can be used to undo the effects of <c>getFish</c> being called repeatedly
/// from the predictor.
/// </remarks>
public interface IFishSideEffect
{
    /// <summary>
    /// Gets whether or not the side effect is understood to apply to the current situation.
    /// </summary>
    /// <remarks>
    /// Side effects that are limited to a specific location or depend on the current player can be
    /// skipped during predictions, improving performance.
    /// </remarks>
    /// <param name="who">The player who is fishing.</param>
    /// <param name="location">The player's current location.</param>
    /// <returns><c>true</c> if predictions for the specified farmer and location require the side
    /// effect to be detected and undone; <c>false</c> to skip checking for this player/location.
    /// </returns>
    bool AppliesTo(Farmer who, GameLocation location);

    /// <summary>
    /// Captures the current state so that it can be later reverted with <see cref="Undo"/>.
    /// </summary>
    /// <remarks>
    /// This is called before every <see cref="GameLocation.getFish"/> and should always be free of
    /// its own side effects.
    /// </remarks>
    /// <param name="who">The player who is fishing.</param>
    void Snapshot(Farmer who);

    /// <summary>
    /// Reverts to the last state captured from <see cref="Snapshot"/>.
    /// </summary>
    /// <remarks>
    /// This is called after every <see cref="GameLocation.getFish"/>.
    /// </remarks>
    /// <param name="who">The player who is fishing.</param>
    void Undo(Farmer who);
}

/// <summary>
/// Side effect that limits golden walnut drops from island fishing.
/// </summary>
/// <remarks>
/// This state is stored globally in the <see cref="FarmerTeam"/> and incremented automatically when
/// fishing a walnut, and must be reverted in order to avoid "losing" those drops.
/// </remarks>
public class NutDropSideEffect : IFishSideEffect
{
    private int foundCount;

    public bool AppliesTo(Farmer who, GameLocation location)
    {
        return location is IslandLocation;
    }

    public void Snapshot(Farmer who)
    {
        foundCount = who.team.limitedNutDrops.TryGetValue("IslandFishing", out var count)
            ? count
            : 0;
    }

    public void Undo(Farmer who)
    {
        if (
            who.team.limitedNutDrops.TryGetValue("IslandFishing", out var count)
            && count > foundCount
        )
        {
            who.team.limitedNutDrops["IslandFishing"] = foundCount;
        }
    }
}
