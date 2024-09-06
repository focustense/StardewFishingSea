using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace FishingBuddy;

internal record Splash(Point Tile, int StartTimeOfDay, int EndTimeOfDay, string? FrenzyFishId);

internal static class SplashPredictor
{
    record SplashStart(Point Tile, int StartTimeOfDay, int MinimumDuration, string? FrenzyFishId)
    {
        public Splash ToSplash(int endTimeOfDay)
        {
            return new(Tile, StartTimeOfDay, endTimeOfDay, FrenzyFishId);
        }
    }

    public static IEnumerable<Splash> PredictSplashes(GameLocation location)
    {
        if (!location.IsOutdoors || (location is Farm && !SplashRules.AllowedOnFarm()))
        {
            yield break;
        }
        SplashStart? currentSplash = null;
        var mapSize = location.Map.RequireLayer("Back").LayerSize;
        var frenzyThreshold = SplashRules.GetFrenzyThreshold(location);
        for (
            int timeOfDay = GameRules.StartOfDay;
            timeOfDay < GameRules.EndOfDay;
            timeOfDay = Utility.ModifyTime(timeOfDay, 10)
        )
        {
            var random = Utility.CreateDaySaveRandom(timeOfDay, location.Map.Layers[0].LayerWidth);
            if (currentSplash is not null)
            {
                int currentDuration = Utility.CalculateMinutesBetweenTimes(
                    currentSplash.StartTimeOfDay,
                    timeOfDay
                );
                // Game code does the random sample before checking against the minimum duration, so
                // we have to do the same.
                var sample = random.NextDouble();
                var threshold =
                    SplashRules.SplashEndThreshold + (currentDuration / SplashRules.DurationWeight);
                if (sample < threshold && currentDuration > currentSplash.MinimumDuration)
                {
                    yield return currentSplash.ToSplash(timeOfDay);
                    currentSplash = null;
                }
                continue;
            }
            // No splash in progress, check if a new one will be created.
            if (!random.NextBool())
            {
                // Note that in the game logic, the NextBool check is done before the farm rule
                // check. However, does not really matter in this case because failing that check
                // means the splash can NEVER be produced at the given location, and the RNG is
                // instanced per location.
                //
                // If we were trying to predict every possible thing about a location (e.g. panning)
                // then we'd need to move the farm-check farther down because the same random
                // instance is reused; but for now, we aren't, and the random seed is recreated in
                // every `GameLocation.performTenMinuteUpdate` loop so there is no need to run it
                // through the other RNG dependencies in this one to stay consistent.
                continue;
            }
            // Why the game does multiple "tries" picking random splash spots instead of just
            // sampling from valid splash spots, who knows, but since it changes the distribution,
            // we must replicate it.
            for (int tries = 0; tries < 2; tries++)
            {
                var x = random.Next(0, mapSize.Width);
                var y = random.Next(0, mapSize.Height);
                if (
                    !location.isOpenWater(x, y)
                    || location.doesTileHaveProperty(x, y, "NoFishing", "Back") != null
                )
                {
                    continue;
                }
                int distanceToLand = FishingRod.distanceToLand(x, y, location);
                if (distanceToLand < 1 || distanceToLand > SplashRules.MaximumDistanceToLand)
                {
                    continue;
                }
                // Frenzy check. As with several other paths, the random samples are taken before
                // certain overriding rules are checked.
                string? frenzyFishId = null;
                var frenzySample = random.NextDouble();
                if (frenzySample < frenzyThreshold && SplashRules.IsFrenzyAllowed(location))
                {
                    var fish = location.getFish(
                        random.Next(500),
                        "",
                        distanceToLand,
                        Game1.player,
                        0.0,
                        new(x, y)
                    );
                    frenzyFishId = fish?.QualifiedItemId;
                }
                var minDuration = frenzyFishId is not null
                    ? SplashRules.MinimumFrenzyDuration
                    : SplashRules.MinimumNonFrenzyDuration;
                currentSplash = new(new(x, y), timeOfDay, minDuration, frenzyFishId);
            }
        }
        if (currentSplash is not null)
        {
            yield return currentSplash.ToSplash(GameRules.EndOfDay);
        }
    }
}
