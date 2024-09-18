using System.Reflection;
using FishingBuddy.Predictions;
using HarmonyLib;

namespace FishingBuddy.Patches;

/// <summary>
/// Central lookup for reflected members used by patches.
/// </summary>
internal class Members
{
    /// <summary>
    /// Property getter for for <see cref="FishRandom.Instance"/>.
    /// </summary>
    public static readonly MethodInfo FishRandomGetMethod = AccessTools.PropertyGetter(
        typeof(FishRandom),
        nameof(FishRandom.Instance)
    );

    /// <summary>
    /// Field reference for <see cref="Game1.random"/>.
    /// </summary>
    public static readonly FieldInfo GameRandomField = AccessTools.Field(
        typeof(Game1),
        nameof(Game1.random)
    );

    /// <summary>
    /// Property getter for <see cref="PatchState.SpeedMultiplier"/>.
    /// </summary>
    public static readonly MethodInfo GetSpeedMultiplierMethod = AccessTools.PropertyGetter(
        typeof(PatchState),
        nameof(PatchState.SpeedMultiplier)
    );

    /// <summary>
    /// Property getter for <see cref="TimeSpan.Milliseconds"/>.
    /// </summary>
    public static readonly MethodInfo TimeSpanGetMillisecondsMethod = AccessTools.PropertyGetter(
        typeof(TimeSpan),
        nameof(TimeSpan.Milliseconds)
    );
}
