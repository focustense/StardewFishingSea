using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

namespace FishingBuddy;

/// <summary>
/// Visibility of the fish catch previews drawn on water tiles.
/// </summary>
public enum CatchPreviewVisibility
{
    /// <summary>
    /// Previews are always visible once activated.
    /// </summary>
    Always,

    /// <summary>
    /// Previews are only visible when the player is using a fishing rod.
    /// </summary>
    OnlyWhenRodSelected,
}

/// <summary>
/// Corners of a rectangular area, e.g. the screen.
/// </summary>
public enum RectangleCorner
{
    /// <summary>
    /// The top left.
    /// </summary>
    TopLeft,

    /// <summary>
    /// The top right.
    /// </summary>
    TopRight,

    /// <summary>
    /// The bottom right.
    /// </summary>
    BottomRight,

    /// <summary>
    /// The bottom left.
    /// </summary>
    BottomLeft,
}

/// <summary>
/// Visibility of the seeded-random fish HUD ("jelly indicator").
/// </summary>
public enum SeededRandomFishHudVisibility
{
    /// <summary>
    /// Don't show the seeded-random fish HUD.
    /// </summary>
    None,

    /// <summary>
    /// Show the HUD only when the next catch can be a seeded-random fish.
    /// </summary>
    Current,

    /// <summary>
    /// Show the HUD whenever the player is in a location where seeded-random fish can spawn, and
    /// if not spawnable on the next catch, include the number of remaining catches required.
    /// </summary>
    CurrentAndFuture,
}

/// <summary>
/// Visibility of the balloons (speech bubbles) over splash points.
/// </summary>
public enum SplashPreviewVisibility
{
    /// <summary>
    /// Don't show any splash info.
    /// </summary>
    None,

    /// <summary>
    /// Only show remaining time on splashes already started; don't show future splashes.
    /// </summary>
    Remaining,

    /// <summary>
    /// Show remaining time on current splashes, and countdown for the next splash.
    /// </summary>
    RemainingAndUpcoming,
}

/// <summary>
/// Configuration settings for FishingBuddy.
/// </summary>
public class ModConfig
{
    /// <summary>
    /// Whether to freeze fish randomness when the reel is cast.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Setting this to <c>true</c> will prevent both predicted and actual fish casts from changing
    /// during the cast animation or while waiting for a bite, which can make catching a specific
    /// fish significantly easier and doesn't require good bait/spinners.
    /// </para>
    /// <para>
    /// However, this only freezes the random state; it does not freeze the actual fish. If the
    /// predicted fish becomes invalid e.g. due to the clock moving outside the hours allowed for
    /// catching it, then the fish will still change.
    /// </para>
    /// </remarks>
    public bool CatchFreezeOnCast { get; set; }

    /// <summary>
    /// Radius from the player's location within which to check and show fish-catch previews.
    /// </summary>
    /// <remarks>
    /// Higher values are more cheaty, and excessively high values may degrade performance.
    /// </remarks>
    public int CatchPreviewTileRadius { get; set; } = 8;

    /// <summary>
    /// Keybind for toggling fish-catch previews, and the derandomization logic supporting them.
    /// </summary>
    public KeybindList CatchPreviewToggleKeybind { get; set; } =
        new(new Keybind(SButton.LeftShift, SButton.F));

    /// <summary>
    /// When catch previews are actually visible.
    /// </summary>
    /// <remarks>
    /// This works with the <see cref="CatchPreviewToggleKeybind"/> and doesn't override it.
    /// Previews are never visible when the toggle is OFF; when the toggle is ON, only visible when
    /// the visibility setting specifies it.
    /// </remarks>
    public CatchPreviewVisibility CatchPreviewVisibility { get; set; } =
        CatchPreviewVisibility.OnlyWhenRodSelected;

    /// <summary>
    /// Whether to force a reset of the fishing randomness on every fishing cancellation.
    /// </summary>
    /// <remarks>
    /// Cancellation means reeling in without having hooked any fish.
    /// </remarks>
    public bool CatchResetOnCancel { get; set; } = false;

    /// <summary>
    /// Number of in-game minutes between updates of the fish-catch previews, and the catches
    /// themselves.
    /// </summary>
    /// <remarks>
    /// Currently is expected to be a multiple of 10 (the rate at which the vanilla game clock
    /// actually updates) and will be rounded to the nearest multiple otherwise.
    /// </remarks>
    public int CatchUpdateInterval { get; set; } = 10;

    /// <summary>
    /// Rate at which the game time will run while fishing (waiting for a bite).
    /// </summary>
    /// <remarks>
    /// Single-player only.
    /// </remarks>
    public float FishingTimeScale { get; set; } = 4.0f;

    /// <summary>
    /// Specifies which corner of the screen should display seeded-random fish indicator (e.g. for
    /// jellies).
    /// </summary>
    public RectangleCorner SeededRandomFishHudLocation { get; set; } = RectangleCorner.TopLeft;

    /// <summary>
    /// The offset in pixels from the <see cref="SeededRandomFishHudLocation"/> to display the
    /// seeded-random HUD.
    /// </summary>
    /// <remarks>
    /// Both X and Y values should be positive, and are interpreted relative to the location.
    /// </remarks>
    public Point SeededRandomFishHudOffset { get; set; } = new(0, 100);

    /// <summary>
    /// When the seeded-random fish HUD indicator is visible.
    /// </summary>
    /// <remarks>
    /// Regardless of setting, this HUD will only be visible when fish previews in general are on,
    /// i.e. via the <see cref="CatchPreviewToggleKeybind"/>.
    /// </remarks>
    public SeededRandomFishHudVisibility SeededRandomFishHudVisibility { get; set; } =
        SeededRandomFishHudVisibility.CurrentAndFuture;

    /// <summary>
    /// Configures when splash previews (balloons over current/future bubble spots) are visible.
    /// </summary>
    public SplashPreviewVisibility SplashPreviewVisibility { get; set; } =
        SplashPreviewVisibility.RemainingAndUpcoming;
}
