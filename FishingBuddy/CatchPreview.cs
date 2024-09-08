using Microsoft.Xna.Framework;
using StardewValley.Tools;

namespace FishingBuddy;

/// <summary>
/// Prediction of a single fish catch on a single tile.
/// </summary>
/// <param name="Tile">The fishing tile location.</param>
/// <param name="FishId">Qualified item ID for the fish that will be caught.</param>
internal record CatchPrediction(Point Tile, string FishId);

/// <summary>
/// Predicts the fish catches in a local area.
/// </summary>
/// <param name="configSelector">Function to get the current mod config.</param>
internal class CatchPreview(Func<ModConfig> configSelector)
{
    /// <summary>
    /// Whether or not predictions are enabled.
    /// </summary>
    /// <remarks>
    /// Disabling predictions will also disable
    /// </remarks>
    public bool Enabled
    {
        get => isEnabled;
        set => SetEnabled(value);
    }

    /// <summary>
    /// Whether or not to freeze the predictions as well as the actual fish catches in their current
    /// state, i.e. to skip syncing to <see cref="Game1.random"/> on time or other context changes.
    /// </summary>
    /// <remarks>
    /// Typically, freezing might occur after a cast, so that long fish bite times wouldn't result
    /// the prediction/fish changing during the wait.
    /// </remarks>
    public bool Frozen { get; set; }

    public IReadOnlyList<CatchPrediction> Predictions => tiles;

    private readonly List<CatchPrediction> tiles = [];

    private string? lastBaitId;
    private string? lastLocationName = null;
    private string? lastRodId = null;
    private Point? lastPlayerTile = null;
    private List<string> lastTackleIds = [];
    private int lastUpdateTimeOfDay;
    private bool isEnabled;

    /// <summary>
    /// Updates predictions for the current time/location.
    /// </summary>
    /// <param name="forceImmediateUpdate">Whether to immediately sync the fishing random state to
    /// the game's current random state, regardless of whether enough time has passed.</param>
    public void Update(bool forceImmediateUpdate = false)
    {
        if (!isEnabled || !Game1.currentLocation.canFishHere())
        {
            return;
        }
        var config = configSelector();
        if (config.CatchPreviewTileRadius == 0)
        {
            return;
        }
        var locationName = Game1.currentLocation.NameOrUniqueName;
        var playerTile = Game1.player.TilePoint;
        var (rodId, baitId, tackleIds) = Game1.player.CurrentTool is FishingRod rod
            ? (rod.QualifiedItemId, rod.GetBait()?.QualifiedItemId, rod.GetTackleQualifiedItemIDs())
            : (null, null, []);
        var minutesElapsed = Utility.CalculateMinutesBetweenTimes(
            lastUpdateTimeOfDay,
            Game1.timeOfDay
        );
        if (
            !forceImmediateUpdate
            && locationName == lastLocationName
            && playerTile == lastPlayerTile
            && rodId == lastRodId
            && baitId == lastBaitId
            && tackleIds.SequenceEqual(lastTackleIds)
            && minutesElapsed < config.CatchUpdateInterval
        )
        {
            return;
        }
        var rng = ReplayableRandom.Global;
        if (forceImmediateUpdate || (!Frozen && minutesElapsed >= config.CatchUpdateInterval))
        {
            rng.Snapshot();
        }
        else
        {
            rng.Rewind();
        }
        tiles.Clear();
        var location = Game1.currentLocation;
        var fishingTiles = GetTilesInRadius(playerTile, config.CatchPreviewTileRadius)
            .Where(tile => location.isTileOnMap(tile) && location.isTileFishable(tile.X, tile.Y));
        foreach (var tile in fishingTiles)
        {
            var distanceToLand = FishingRod.distanceToLand(tile.X, tile.Y, location);
            var fish = location.getFish(
                millisecondsAfterNibble: 0.0f, // Unused since a very long time ago
                bait: baitId ?? "", // Unused now, looks like it might get used again
                baitPotency: 0.0f, // Unused and even documented as unused
                waterDepth: distanceToLand, // Looks like SDV repurposed "depth" to "distance"
                who: Game1.player,
                bobberTile: tile.ToVector2()
            );
            if (fish is not null)
            {
                tiles.Add(new(tile, fish.QualifiedItemId));
            }
            rng.Rewind();
        }
        lastLocationName = locationName;
        lastPlayerTile = playerTile;
        lastRodId = rodId;
        lastBaitId = baitId;
        lastTackleIds = tackleIds;
        lastUpdateTimeOfDay = Game1.timeOfDay;
    }

    private static IEnumerable<Point> GetTilesInRadius(Point center, int radius)
    {
        for (int dx = -radius; dx <= radius; dx += 1)
        {
            var dyMax = radius - Math.Abs(dx);
            for (int dy = -dyMax; dy <= dyMax; dy += 1)
            {
                yield return new(center.X + dx, center.Y + dy);
            }
        }
    }

    private void SetEnabled(bool enabled)
    {
        if (isEnabled == enabled)
        {
            return;
        }
        isEnabled = enabled;
        FishRandom.Source = enabled ? FishRandomSource.Replayable : FishRandomSource.Default;
        Update();
    }
}
