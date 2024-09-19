namespace FishinC.Patches;

/// <summary>
/// Shared state for the various patches.
/// </summary>
internal static class PatchState
{
    /// <summary>
    /// Logger for the mod, available as static for patchers. Set in ModEntry.
    /// </summary>
    public static IMonitor? Monitor { get; set; }

    /// <summary>
    /// Current speed multiplier to apply to NPCs and other characters.
    /// </summary>
    public static int SpeedMultiplier { get; set; } = 1;

    /// <inheritdoc cref="IMonitor.Log(string, LogLevel)" />
    public static void Log(string message, LogLevel level = LogLevel.Trace)
    {
        Monitor?.Log(message, level);
    }
}
