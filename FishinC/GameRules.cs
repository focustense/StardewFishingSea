using System.Runtime.CompilerServices;

namespace FishinC;

/// <summary>
/// Assumptions about things that are normally hardcoded in the game.
/// </summary>
/// <remarks>
/// There are, or have been, mods doing insane things like changing the length of a day. We are not
/// going to try to ensure compatibility with all of them in this mod. If they do insane things,
/// they can Harmony-patch these methods to play nice with their insanity.
/// </remarks>
public static class GameRules
{
    /// <summary>
    /// The <see cref="Game1.timeOfDay"/> representing the end of the day, when player passes out.
    /// </summary>
    public static int EndOfDay
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 2600;
    }

    /// <summary>
    /// The <see cref="Game1.timeOfDay"/> upon starting a new day.
    /// </summary>
    public static int StartOfDay
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => 600;
    }
}
