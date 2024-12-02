using PropertyChanged.SourceGenerator;
using StardewValley.ItemTypeDefinitions;

namespace FishinC.UI;

/// <summary>
/// View model for the seeded-random fish HUD.
/// </summary>
public partial class SeedFishInfoViewModel
{
    /// <summary>
    /// Whether more regular fish catches are required in order to catch the specified
    /// <see cref="FishId"/>; i.e. <see cref="CatchesRemaining"/> is not zero.
    /// </summary>
    public bool HasCatchesRemaining => CatchesRemaining > 0;

    /// <summary>
    /// Tint color with which to display the fish image.
    /// </summary>
    public Color FishTintColor => HasCatchesRemaining ? CatchesRemainingTintColor : Color.White;

    private static readonly Color CatchesRemainingTintColor = Color.White * 0.5f;

    /// <summary>
    /// Data for the specified <see cref="FishId"/>, automatically updated.
    /// </summary>
    [Notify(Setter.Private)]
    private ParsedItemData? fishData;

    /// <summary>
    /// Qualified item ID of the fish (i.e. jelly) to display.
    /// </summary>
    [Notify]
    private string fishId = "";

    /// <summary>
    /// Number of catches remaining until the specified fish can be caught.
    /// </summary>
    [Notify]
    private int catchesRemaining;

    private void OnFishIdChanged()
    {
        fishData = !string.IsNullOrEmpty(fishId) ? ItemRegistry.GetDataOrErrorItem(fishId) : null;
    }
}
