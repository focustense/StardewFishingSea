using StardewValley.Locations;

namespace FishinC.Predictions;

/// <summary>
/// Public and internal API for registering side effects.
/// </summary>
public interface ISideEffectsApi
{
    /// <summary>
    /// Registers a new side effect.
    /// </summary>
    void Add(IFishSideEffect sideEffect);
}

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

/// <summary>
/// Side effect for logic dependent on number of times fished.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StardewValley.Tools.FishingRod"/> increments this game stat on every <em>cast</em>,
/// i.e. before any fish is caught; thus any location checking for <see cref="Stats.TimesFished"/>
/// (e.g. for a seeded random check) needs to see the current value + 1 if we bypass the rod and
/// directly call <see cref="GameLocation.getFish"/>.
/// </para>
/// <para>
/// Currently, this is only known to affect the island, and could potentially be part of the
/// <see cref="NutDropSideEffect"/>; however, since it is cheap to run and could easily be included
/// in some modded or future vanilla logic, it's better to run all the time.
/// </para>
/// </remarks>
public class TimesFishedSideEffect : IFishSideEffect
{
    private uint previousTimesFished;

    public bool AppliesTo(Farmer who, GameLocation location)
    {
        return true;
    }

    public void Snapshot(Farmer who)
    {
        previousTimesFished = Game1.stats.TimesFished;
        Game1.stats.TimesFished++;
    }

    public void Undo(Farmer who)
    {
        Game1.stats.TimesFished = previousTimesFished;
    }
}
