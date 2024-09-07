namespace FishingBuddy;

/// <summary>
/// Chooses the source of randomness for fishing-related functions.
/// </summary>
internal enum FishRandomSource
{
    /// <summary>
    /// Use the default <see cref="Game1.random"/> instance.
    /// </summary>
    Default,

    /// <summary>
    /// Use a <see cref="ReplayableRandom"> PRNG forked from the default entropy.
    /// </summary>
    Replayable,
}

/// <summary>
/// Router for fishing-related random samples; used as swappable transpiler target.
/// </summary>
internal static class FishRandom
{
    /// <summary>
    /// The current RNG used for fishing.
    /// </summary>
    public static Random Instance =>
        Source == FishRandomSource.Replayable ? ReplayableRandom.Global : Game1.random;

    /// <summary>
    /// Configures the source for fishing randomness..
    /// </summary>
    public static FishRandomSource Source { get; set; } = FishRandomSource.Default;
}
