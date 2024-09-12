using FishingBuddy.Data;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

namespace FishingBuddy.Configuration;

/// <summary>
/// Configuration settings for FishingBuddy.
/// </summary>
public class ModConfig
{
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
    /// Rate at which the game time will run while fishing (waiting for a bite).
    /// </summary>
    /// <remarks>
    /// Single-player only.
    /// </remarks>
    public float FishingTimeScale { get; set; } = 4.0f;

    /// <summary>
    /// Number of in-game minutes between fish respawns, i.e. when both the predicted fish and
    /// actual catches are updated based on the game's real random state.
    /// </summary>
    /// <remarks>
    /// Currently is expected to be a multiple of 10 (the rate at which the vanilla game clock
    /// actually updates) and will be rounded to the nearest multiple otherwise.
    /// </remarks>
    public int RespawnInterval { get; set; } = 10;

    /// <summary>
    /// The active rule set, whether standard or custom.
    /// </summary>
    public RuleSet Rules { get; set; } = new();

    /// <summary>
    /// Name of the rule set, if using a standard rule set; empty if using custom rules.
    /// </summary>
    public string RuleSetName { get; set; } = "medium";

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
}
