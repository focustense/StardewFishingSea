using FishinC.Configuration;
using FishinC.Patches;
using StardewValley.Tools;

namespace FishinC;

/// <summary>
/// Handles changing the game's time speed and adjusting dependent elements accordingly.
/// </summary>
internal class TimeAccelerator(Func<ModConfig> config)
{
    public bool Active
    {
        get => isActive;
        set
        {
            if (value == isActive)
            {
                return;
            }
            isActive = value;
            if (isActive)
            {
                timeScale = config().FishingTimeScale;
                PatchState.SpeedMultiplier = timeScale;
                Reset();
            }
            else
            {
                PatchState.SpeedMultiplier = 1;
            }
        }
    }

    private bool isActive;
    private float previousProgress;
    private int timeScale;

    /// <summary>
    /// Resets tracked progress, so speed-up does not happen until at least the next frame.
    /// </summary>
    public void Reset()
    {
        previousProgress = Game1.gameTimeInterval;
    }

    /// <summary>
    /// Handles an in-game tick, adjusting the <see cref="Game1.gameTimeInterval"/> as necessary to
    /// accelerate time.
    /// </summary>
    /// <param name="rod">The player's equipped fishing rod; should be specified in order to also
    /// accelerate the bite progress.</param>
    public void Update(FishingRod? rod)
    {
        if (!Active || timeScale == 1)
        {
            return;
        }
        var currentProgress = Game1.gameTimeInterval;
        if (currentProgress == previousProgress)
        {
            return;
        }
        if (currentProgress < previousProgress)
        {
            // Ideally we'd want to scale according to the elapsed time prior to ten-minute rollover
            // and elapsed since the rollover. However, the game itself does not do this; when it's
            // time for a 10-minute update, it simply resets the elapsed time to zero, and does not
            // increase it by the rollover amount.
            previousProgress = 0;
        }
        var deltaProgress = currentProgress - previousProgress;
        var maxProgress =
            Game1.realMilliSecondsPerGameTenMinutes
            + Game1.currentLocation.ExtraMillisecondsPerInGameMinute * 10;
        var nextProgress = MathF.Min(previousProgress + deltaProgress * timeScale, maxProgress - 1);
        if (rod is not null)
        {
            var extraProgress = nextProgress - currentProgress;
            rod.fishingBiteAccumulator += extraProgress;
        }
        previousProgress = Game1.gameTimeInterval = (int)MathF.Round(nextProgress);
    }
}
