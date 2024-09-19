using System.Runtime.CompilerServices;
using StardewValley.Locations;

namespace FishinC.Predictions;

/// <summary>
/// Normally-hardcoded rules specific to splash spots (fishing bubbles).
/// </summary>
/// <remarks>
/// Based on same compatibility philosophy as <see cref="GameRules"/>.
/// </remarks>
public static class SplashRules
{
    /// <summary>
    /// Divisor applied to the current splash duration, which is then added to the random threshold
    /// for stopping bubbles.
    /// </summary>
    /// <remarks>
    /// The higher the value, the subtler the effect of elapsed splash time on whether the splash
    /// will disappear, and the more truly random it is whether the splash ends at any particular
    /// time of day.
    /// </remarks>
    public static float DurationWeight
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 1800f;
    }

    /// <summary>
    /// Latest (maximum) time of day for a frenzy, after which frenzies are blocked.
    /// </summary>
    public static int MaximumFrenzyStartTimeOfDay
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 2300;
    }

    /// <summary>
    /// Maximum distance in tiles that a splash spot can appear from land.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value seems to be chosen to allow most splashes to be reached at level 0.
    /// "Most", and not "all", because North/South casts are 1 tile shorter than East/West, and some
    /// diagonal spots that appear in game are simply not reachable, period.
    /// </para>
    /// <para>
    /// Note that the check seen in game decompilation is <c>>= 5</c> but that is the condition for
    /// <em>skipping</em> the bubbles, meaning that the "maximum" is actually 4; any value farther
    /// than 4 tiles won't spawn bubbles.
    /// </para>
    /// </remarks>
    public static int MaximumSplashDistanceToLand
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 4;
    }

    /// <summary>
    /// Minimum number of in-game days that must have elapsed before an "early" frenzy can occur,
    /// which only applies if the <see cref="MinimumFishCaughtBeforeEarlyFrenzy"/> threshold has
    /// also been met.
    /// </summary>
    /// <remarks>
    /// The current <see cref="WorldDate.TotalDays"/> must be <em>greater</em> than this value.
    /// </remarks>
    public static int MinimumDaysBeforeEarlyFrenzy
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 3;
    }

    /// <summary>
    /// Minimum number of in-game days that must have elapsed before a "late" frenzy can occur,
    /// which applies regardless of how many fish the player has caught.
    /// </summary>
    /// <remarks>
    /// The current <see cref="WorldDate.TotalDays"/> must be <em>greater</em> than this value.
    /// </remarks>
    public static int MinimumDaysBeforeLateFrenzy
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 14;
    }

    /// <summary>
    /// Minimum number of fish that the player must have caught before an "early" frenzy can occur,
    /// as long as the <see cref="MinimumDaysBeforeEarlyFrenzy"/> have also elapsed.
    /// </summary>
    /// <remarks>
    /// The number of fish caught must be <em>greater than or equal to</em> this value, so it is 1
    /// higher than the decompilation value that does a greater-than-check.
    /// </remarks>
    public static int MinimumFishCaughtBeforeEarlyFrenzy
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 3;
    }

    /// <summary>
    /// Minimum time that a splash spot will last when a frenzy is taking place.
    /// </summary>
    public static int MinimumFrenzyDuration
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 120;
    }

    /// <summary>
    /// Minimum time that a splash spot will last without an accompanying frenzy.
    /// </summary>
    public static int MinimumNonFrenzyDuration
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 60;
    }

    /// <summary>
    /// Threshold (maximum) value for the RNG to end a current splash spot, before accounting for
    /// the duration and <see cref="DurationWeight"/>.
    /// </summary>
    public static double SplashEndThreshold
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 0.1f;
    }

    /// <summary>
    /// Gets whether or not splash spots (fishing bubbles) are allowed on Farm maps.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool AllowedOnFarm()
    {
        return Game1.whichFarm == 1; // Riverland Farm
    }

    /// <summary>
    /// Gets the maximum random value to pass a frenzy check and trigger a frenzy for a splash spot.
    /// </summary>
    /// <remarks>
    /// This is further restricted by <see cref="IsFrenzyAllowed"/>.
    /// </remarks>
    /// <param name="location">The location where the splash spot will spawn.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static double GetFrenzyThreshold(GameLocation location)
    {
        return location is Beach ? 0.008 : 0.01;
    }

    /// <summary>
    /// Checks if a fish frenzy can spawn in a given location after passing the random check.
    /// </summary>
    /// <remarks>
    /// This puts together all the individual frenzy rules and thresholds; patch this if a mod has
    /// completely changed the entire algorithm as opposed to tweaking individual rules.
    /// </remarks>
    /// <param name="location">The location where the frenzy might start.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsFrenzyAllowed(GameLocation location)
    {
        return Game1.Date.TotalDays > MinimumDaysBeforeEarlyFrenzy
            && (location is Town || location is Mountain || location is Forest || location is Beach)
            && Game1.timeOfDay < MaximumFrenzyStartTimeOfDay
            && (
                Game1.player.fishCaught.Count() >= MinimumFishCaughtBeforeEarlyFrenzy
                || Game1.Date.TotalDays > MinimumDaysBeforeLateFrenzy
            )
            && !Utility.isFestivalDay();
    }
}
