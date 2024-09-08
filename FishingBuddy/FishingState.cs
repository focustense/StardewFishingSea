using StardewValley.Tools;

namespace FishingBuddy;

/// <summary>
/// Tracks the current state of the fishing minigame, providing state changes as events.
/// </summary>
internal class FishingState
{
    /// <summary>
    /// Fishing was cancelled, i.e. pulled in without any catch.
    /// </summary>
    public event EventHandler<EventArgs>? Cancelled;

    /// <summary>
    /// Line was cast; minigame has not started yet.
    /// </summary>
    public event EventHandler<EventArgs>? Cast;

    /// <summary>
    /// A fish was caught.
    /// </summary>
    /// <remarks>
    /// Fired as soon as the "pulling" animation starts.
    /// </remarks>
    public event EventHandler<EventArgs>? Caught;

    /// <summary>
    /// A fish got away (player "lost" the minigame).
    /// </summary>
    public event EventHandler<EventArgs>? Lost;

    enum Status
    {
        Idle,
        Casting,
        Fishing,
        Reeling,
        Cancelling,
        Catching,
    }

    private Status currentStatus = Status.Idle;

    /// <summary>
    /// Updates the state based on a selected fishing rod.
    /// </summary>
    /// <param name="rod">The rod that the player has currently equipped.</param>
    public void Update(FishingRod? rod)
    {
        var isCasting = rod?.isCasting ?? false;
        var isCastingAnimation = rod?.castedButBobberStillInAir ?? false;
        var isFishing = rod?.isFishing ?? false;
        var isNibbling = rod?.isNibbling ?? false;
        var isReeling = rod?.isReeling ?? false;
        var isTimingCast = rod?.isTimingCast ?? false;
        var isPulling = rod?.pullingOutOfWater ?? false;
        var isCaught = rod?.fishCaught ?? false;

        if (currentStatus == Status.Idle && (isTimingCast || isCasting || isCastingAnimation))
        {
            Cast?.Invoke(this, EventArgs.Empty);
            currentStatus = Status.Casting;
        }
        if (currentStatus == Status.Casting && isFishing)
        {
            currentStatus = Status.Fishing;
        }
        if (currentStatus == Status.Fishing && isReeling)
        {
            currentStatus = Status.Reeling;
        }
        // This looser logic should, hopefully, work with other fishing mods that do instant
        // catches. But who knows what crazy edits they make.
        var isDoneStatus = currentStatus == Status.Cancelling || currentStatus == Status.Catching;
        if (!isDoneStatus)
        {
            if (isCaught || (isPulling && isNibbling))
            {
                Caught?.Invoke(this, EventArgs.Empty);
                currentStatus = Status.Catching;
            }
            else if (isPulling && !isNibbling)
            {
                Cancelled?.Invoke(this, EventArgs.Empty);
                currentStatus = Status.Cancelling;
            }
        }

        if (
            currentStatus != Status.Idle
            && !isCasting
            && !isCastingAnimation
            && !isFishing
            && !isNibbling
            && !isReeling
            && !isTimingCast
            && !isCaught
            && !isPulling
        )
        {
            if (currentStatus == Status.Reeling)
            {
                Lost?.Invoke(this, EventArgs.Empty);
            }
            else if (!isDoneStatus)
            {
                Cancelled?.Invoke(this, EventArgs.Empty);
            }
            currentStatus = Status.Idle;
        }
    }
}
