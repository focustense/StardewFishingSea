using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;

namespace FishinC.UI;

using Edges = (int left, int top, int right, int bottom);

/// <summary>
/// View model for the balloon displaying info about a splash spot (bubbles).
/// </summary>
public partial class SplashInfoViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Specifies the position and tint of a single fish "layer" on the visual "stack".
    /// </summary>
    /// <param name="Margin">Margin to apply to the image, i.e. its position offset.</param>
    /// <param name="Tint">Image tint, mainly for opacity of lower/farther fish.</param>
    public record FishLayer(Edges Margin, Color Tint);

    private const int FRENZY_SPRITE_COUNT = 5;
    private const int SPLASH_SPRITE_COUNT = 3;

    private static readonly Rectangle GenericFishSourceRect = new(615, 1841, 19, 19);

    /// <summary>
    /// Color with which to display the <see cref="DurationText"/>.
    /// </summary>
    [Notify(Setter.Private)]
    private Color durationColor;

    /// <summary>
    /// The duration in minutes; used to set the <see cref="DurationText"/>.
    /// </summary>
    [Notify]
    private int durationMinutes;

    /// <summary>
    /// Formatted text for the duration remaining until the spot appears or disappears.
    /// </summary>
    [Notify(Setter.Private)]
    private string durationText = "";

    /// <summary>
    /// Positions and tints of the fish to display representing the splash details (e.g. regular
    /// splash vs. frenzies).
    /// </summary>
    [Notify]
    private IReadOnlyList<FishLayer> fish = [];

    /// <summary>
    /// The sprite to display for each of the <see cref="Fish"/>. Either a generic fish image or,
    /// for frenzies, the image of the frenzying fish.
    /// </summary>
    [Notify]
    private Tuple<Texture2D, Rectangle> fishSprite = GetGenericFishSprite();

    /// <summary>
    /// Unique item ID for the frenzying fish, if any.
    /// </summary>
    [Notify]
    private string? frenzyFishId;

    /// <summary>
    /// Initializes a new <see cref="SplashInfoViewModel"/> instance.
    /// </summary>
    public SplashInfoViewModel()
    {
        UpdateFishLayers();
    }

    private static Tuple<Texture2D, Rectangle> GetFishSprite(string? fishId)
    {
        if (string.IsNullOrEmpty(fishId) || ItemRegistry.GetData(fishId) is not { } fishData)
        {
            return GetGenericFishSprite();
        }
        return Tuple.Create(fishData.GetTexture(), fishData.GetSourceRect());
    }

    private static Tuple<Texture2D, Rectangle> GetGenericFishSprite()
    {
        return Tuple.Create(Game1.mouseCursors, GenericFishSourceRect);
    }

    private void OnDurationMinutesChanged()
    {
        // Duration as derived from e.g. Utility.CalculateMinutesBetweenTimes should be in real
        // in-game minutes and not the quirky 26-hour time format.
        var hours = Math.Abs(DurationMinutes) / 60;
        var minutes = Math.Abs(DurationMinutes) % 60;
        DurationText = $"{hours}:{minutes:D2}";
        DurationColor =
            durationMinutes < 0 ? Color.Blue
            : durationMinutes < 60 ? Color.Red
            : Color.DarkGreen;
    }

    private void OnFrenzyFishIdChanged()
    {
        FishSprite = GetFishSprite(frenzyFishId);
        UpdateFishLayers();
    }

    private void UpdateFishLayers()
    {
        var count = !string.IsNullOrEmpty(frenzyFishId) ? FRENZY_SPRITE_COUNT : SPLASH_SPRITE_COUNT;
        if (Fish.Count == count)
        {
            return;
        }
        Func<int, Edges> marginSelector = frenzyFishId is not null
            ? i => new(i * 8, 0, 0, i % 2 * 16 - i % 3 * 4)
            : i => new(i * 4, 0, 0, i * 4);
        var fishIcons = new FishLayer[count];
        for (int i = 0; i < count; i++)
        {
            var margin = marginSelector(i);
            var tint = new Color(Vector4.One * (1 - 0.08f * i));
            fishIcons[count - i - 1] = new(margin, tint);
        }
        Fish = fishIcons;
    }
}
