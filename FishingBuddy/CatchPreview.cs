using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.ItemTypeDefinitions;
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

    /// <summary>
    /// The current fish catch predictions, per nearby tile.
    /// </summary>
    public IReadOnlyList<CatchPrediction> Predictions => tiles;

    /// <summary>
    /// Number of fish that must be caught before there is another chance at catching one of the
    /// <see cref="SeededRandomFish"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the value is <c>0</c>, then the <see cref="Predictions"/> will eventually include one of
    /// these fish (if it doesn't already), after some number of <see cref="Update"/>s, either due
    /// to natural passing of time or forced immediate updates. If the value is anything other than
    /// <c>0</c>, then none of the <see cref="SeededRandomFish"/> fish can be caught on this "turn"
    /// and other fish must be caught first.
    /// </para>
    /// <para>
    /// A value of <c>-1</c> or any negative value means that we failed to calculate the remaining
    /// count, but still confirms that the seeded-random fish cannot be caught or appear in
    /// <see cref="Predictions"/> until other fish are caught. It has the same meaning as a positive
    /// value, but the exact quantity is unknown.
    /// </para>
    /// </remarks>
    public int SeededRandomCatchesRequired { get; private set; } = -1;

    /// <summary>
    /// Data for the types of fish in the current location that must pass a seed-random check;
    /// generally one of the jellies (River, Sea, Cave).
    /// </summary>
    /// <remarks>
    /// With vanilla game data, the list should typically either be empty or contain a single
    /// element; however, <see cref="LocationData"/> technically allows any number of these.
    /// </remarks>
    public IReadOnlyList<ParsedItemData> SeededRandomFish { get; private set; } = [];

    private const int SEEDED_RANDOM_ATTEMPTS = 20;

    private readonly List<SpawnFishData> seededRandomSpawns = [];
    private readonly List<CatchPrediction> tiles = [];

    private PreviewInputs? lastInputs;
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
        var inputs = PreviewInputs.FromCurrentGameState();
        if (forceImmediateUpdate || !inputs.SameForSeededRandomCatches(lastInputs))
        {
            UpdateSeededRandomFish(inputs);
        }
        var minutesElapsed = inputs.MinutesElapsedSince(lastInputs);
        if (
            forceImmediateUpdate
            || !inputs.SameForRegularCatches(lastInputs)
            || minutesElapsed >= config.CatchUpdateInterval
        )
        {
            bool snapshot =
                forceImmediateUpdate || (!Frozen && minutesElapsed >= config.CatchUpdateInterval);
            UpdateRegularFish(inputs, config.CatchPreviewTileRadius, snapshot);
        }
        lastInputs = inputs;
    }

    private static float ApplyDefaultQuantityModifiers(
        float value,
        IList<QuantityModifier> modifiers,
        QuantityModifier.QuantityModifierMode mode
    )
    {
        return Utility.ApplyQuantityModifiers(value, modifiers, mode, Game1.currentLocation);
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

    private void UpdateRegularFish(PreviewInputs inputs, int radius, bool snapshot)
    {
        var rng = ReplayableRandom.Global;
        if (snapshot)
        {
            rng.Snapshot();
        }
        else
        {
            rng.Rewind();
        }
        tiles.Clear();
        var location = Game1.currentLocation;
        var fishingTiles = GetTilesInRadius(inputs.PlayerTile, radius)
            .Where(tile => location.isTileOnMap(tile) && location.isTileFishable(tile.X, tile.Y));
        foreach (var tile in fishingTiles)
        {
            var distanceToLand = FishingRod.distanceToLand(tile.X, tile.Y, location);
            var fish = location.getFish(
                millisecondsAfterNibble: 0.0f, // Unused since a very long time ago
                bait: inputs.BaitId ?? "", // Unused now, looks like it might get used again
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
    }

    private void UpdateSeededRandomFish(PreviewInputs inputs)
    {
        SeededRandomFish = [];
        SeededRandomCatchesRequired = -1;
        if (inputs.LocationName != lastInputs?.LocationName)
        {
            seededRandomSpawns.Clear();
            var locationData = Game1.currentLocation.GetData();
            if (locationData is null)
            {
                return;
            }
            var seededRandomFish = locationData.Fish.Where(f =>
                f.UseFishCaughtSeededRandom
                // Spawns can be either a qualified ID or a query. Vanilla data doesn't use queries
                // for any known seeded spawns and we don't handle them here.
                && ItemRegistry.IsQualifiedItemId(f.ItemId)
            );
            seededRandomSpawns.AddRange(seededRandomFish);
        }
        if (seededRandomSpawns.Count == 0)
        {
            return;
        }
        var possibleCatches = new List<ParsedItemData>();
        var hasCuriosityLure = inputs.TackleIds.Contains("(O)856");
        foreach (var spawn in seededRandomSpawns)
        {
            float chance = spawn.GetChance(
                hasCuriosityLure,
                inputs.DailyLuck,
                inputs.LuckLevel,
                ApplyDefaultQuantityModifiers,
                spawn.ItemId == inputs.BaitTargetFishId
            );
            // We only show the preview for the "earliest" seeded catch, so only iterate up to the
            // same count as previously discovered (if known). That is, it may be useful to know if
            // multiple "special" fish can be caught on the same turn, but ignore any that would be
            // caught only on subsequent turns.
            var maxIterations =
                SeededRandomCatchesRequired >= 0
                    ? SeededRandomCatchesRequired
                    : SEEDED_RANDOM_ATTEMPTS;
            for (int i = 0; i <= maxIterations; i++)
            {
                var seedRandom = Utility.CreateRandom(
                    Game1.uniqueIDForThisGame,
                    (inputs.CatchCount + i) * 859
                );
                if (seedRandom.NextBool(chance))
                {
                    possibleCatches.Add(ItemRegistry.GetDataOrErrorItem(spawn.ItemId));
                    SeededRandomCatchesRequired = i;
                    break;
                }
            }
        }
        SeededRandomFish = possibleCatches;
    }

    private record PreviewInputs(
        string LocationName,
        int TimeOfDay,
        int LuckLevel,
        double DailyLuck,
        string? RodId,
        string? BaitId,
        string? BaitTargetFishId,
        IReadOnlyList<string> TackleIds,
        uint CatchCount,
        Point PlayerTile
    )
    {
        public static PreviewInputs FromCurrentGameState()
        {
            var locationName = Game1.currentLocation.NameOrUniqueName;
            var player = Game1.player;
            var catchCount = player.stats.Get("PreciseFishCaught");
            var (rodId, baitId, baitTargetFishId, tackleIds) = player.CurrentTool is FishingRod rod
                ? (
                    rod.QualifiedItemId,
                    rod.GetBait()?.QualifiedItemId,
                    GetTargetFishId(rod.GetBait()),
                    rod.GetTackleQualifiedItemIDs()
                )
                : (null, null, null, []);
            return new(
                locationName,
                Game1.timeOfDay,
                player.LuckLevel,
                player.DailyLuck,
                rodId,
                baitId,
                baitTargetFishId,
                tackleIds,
                catchCount,
                player.TilePoint
            );
        }

        public int MinutesElapsedSince(PreviewInputs? other)
        {
            return Utility.CalculateMinutesBetweenTimes(other?.TimeOfDay ?? 0, TimeOfDay);
        }

        public bool SameForRegularCatches(PreviewInputs? other)
        {
            return SameForAnyCatches(other) && PlayerTile == other.PlayerTile;
        }

        public bool SameForSeededRandomCatches(PreviewInputs? other)
        {
            return SameForAnyCatches(other) && other.CatchCount == CatchCount;
        }

        private bool SameForAnyCatches([NotNullWhen(true)] PreviewInputs? other)
        {
            return other is not null
                && LocationName == other.LocationName
                && LuckLevel == other.LuckLevel
                && DailyLuck == other.DailyLuck
                && RodId == other.RodId
                && BaitId == other.BaitId
                && BaitTargetFishId == other.BaitTargetFishId
                && TackleIds.SequenceEqual(other.TackleIds);
        }

        private static string? GetTargetFishId(SObject? bait)
        {
            return
                bait?.QualifiedItemId == "(O)SpecificBait"
                && bait.preservedParentSheetIndex.Value is not null
                ? "(O)" + bait.preservedParentSheetIndex.Value
                : null;
        }
    }
}
