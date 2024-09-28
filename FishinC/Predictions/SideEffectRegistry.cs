namespace FishinC.Predictions;

/// <summary>
/// Internal registry of side effects, also implements the API.
/// </summary>
internal class SideEffectRegistry(IMonitor monitor) : ISideEffectsApi
{
    private readonly List<IFishSideEffect> sideEffects =
    [
        new TimesFishedSideEffect(),
        new FishPondSideEffect(),
        new NutDropSideEffect(),
    ];

    public void Add(IFishSideEffect sideEffect)
    {
        sideEffects.Add(sideEffect);
    }

    /// <summary>
    /// Starts a new scope that will undo accumulated side effects once completed.
    /// </summary>
    /// <remarks>
    /// Snapshots for each side effects are taking immediately on invocation. The effects are then
    /// undone when the <see cref="IDisposable"/> result is disposed.
    /// </remarks>
    /// <param name="who">The current player.</param>
    /// <param name="location">The player's current location.</param>
    /// <param name="tile">Tile where the prediction will be done.</param>
    /// <returns>An <see cref="IDisposable"/> which, when disposed, will undo any known side effects
    /// that occurred during or after the snapshots.</returns>
    public IDisposable BeginScope(Farmer who, GameLocation location, Point tile)
    {
        var possibleSideEffects = sideEffects.Where(e => e.AppliesTo(who, location, tile));
        var scopedSideEffects = new List<IFishSideEffect>(sideEffects.Count);
        foreach (var sideEffect in possibleSideEffects)
        {
            try
            {
                // If a side effect fails to snapshot, then trying to undo it would only corrupt
                // game state even further, so only track the ones that succeeded.
                sideEffect.Snapshot(who, location, tile);
                scopedSideEffects.Add(sideEffect);
            }
            catch (Exception ex)
            {
                // This runs for every prediction on every update tick, so don't spam log messages
                // if one is consistently failing.
                monitor.LogOnce(
                    $"Unexpected error while snapshotting {sideEffect.GetType().Name}: {ex}",
                    LogLevel.Error
                );
            }
        }
        return new SideEffectScope(scopedSideEffects, monitor);
    }
}

/// <summary>
/// Result of <see cref="SideEffectRegistry.BeginScope"/>. Undoes side effects on dispose.
/// </summary>
/// <param name="sideEffects">The side effects tracked in this scope.</param>
/// <param name="monitor">Logger instance for the mod.</param>
class SideEffectScope(IReadOnlyList<IFishSideEffect> sideEffects, IMonitor monitor) : IDisposable
{
    public void Dispose()
    {
        foreach (var sideEffect in sideEffects)
        {
            // While it's very bad to fail undoing even one side effect, it's even worse to fail all
            // of them because one of them failed.
            try
            {
                sideEffect.Undo(Game1.player);
            }
            catch (Exception ex)
            {
                // This runs for every prediction on every update tick, so don't spam log messages
                // if one is consistently failing.
                monitor.LogOnce(
                    $"Unexpected error while undoing {sideEffect.GetType().Name}: {ex}",
                    LogLevel.Error
                );
            }
        }
        GC.SuppressFinalize(this);
    }
}
