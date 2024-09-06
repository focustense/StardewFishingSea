using Microsoft.Xna.Framework;
using StardewUI;

namespace FishingBuddy.UI;

/// <summary>
/// Displays info about a splash point (fishing bubbles).
/// </summary>
internal class SplashInfoView : WrapperView
{
    /// <summary>
    /// The time remaining for the splash, if positive; or the time until the splash starts, if
    /// negative.
    /// </summary>
    public int DurationMinutes
    {
        get => durationMinutes;
        set
        {
            if (durationMinutes == value)
            {
                return;
            }
            durationMinutes = value;
            UpdateDurationLabels();
        }
    }

    /// <summary>
    /// The type of fish that is frenzying, if the splash is identified as a frenzy.
    /// </summary>
    public string? FrenzyFishId
    {
        get => frenzyFishId;
        set
        {
            if (frenzyFishId == value)
            {
                return;
            }
            frenzyFishId = value;
            UpdateImagePanel();
        }
    }

    private const int FRENZY_SPRITE_COUNT = 5;
    private const int SPLASH_SPRITE_COUNT = 3;

    private string? frenzyFishId;
    private int durationMinutes;

    private Label? durationLabel;
    private Panel? imagePanel = null!;

    protected override IView CreateView()
    {
        imagePanel = new Panel()
        {
            Layout = LayoutParameters.FixedSize(80, 80),
            HorizontalContentAlignment = Alignment.Start,
            VerticalContentAlignment = Alignment.End,
        };
        UpdateImagePanel();
        var durationImage = new Image()
        {
            Layout = LayoutParameters.FixedSize(36, 36),
            Margin = new(Right: 8),
            Sprite = Sprites.TinyClock,
            ShadowAlpha = 0.5f,
            ShadowOffset = new(-2, 2),
        };
        durationLabel = new Label()
        {
            Layout = LayoutParameters.FitContent(),
            Font = Game1.dialogueFont,
            Bold = true,
        };
        var durationLane = new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = new(Top: 8),
            VerticalContentAlignment = Alignment.Middle,
            Children = [durationImage, durationLabel],
        };
        UpdateDurationLabels();
        return new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = Alignment.Middle,
            Children = [imagePanel, durationLane],
        };
    }

    private void UpdateDurationLabels()
    {
        if (durationLabel is null)
        {
            return;
        }
        // Duration as derived from e.g. Utility.CalculateMinutesBetweenTimes should be in real
        // in-game minutes and not the quirky 26-hour time format.
        var hours = Math.Abs(durationMinutes) / 60;
        var minutes = Math.Abs(durationMinutes) % 60;
        durationLabel.Text = $"{hours}:{minutes:D2}";
        durationLabel.Color =
            durationMinutes < 0 ? Color.Blue
            : durationMinutes < 60 ? Color.Red
            : Color.DarkGreen;
    }

    private void UpdateImagePanel()
    {
        if (imagePanel is null)
        {
            return;
        }
        var sprite = frenzyFishId is not null ? Sprites.Item(frenzyFishId) : Sprites.GenericFish;
        var count = frenzyFishId is not null ? FRENZY_SPRITE_COUNT : SPLASH_SPRITE_COUNT;
        Func<int, Edges> marginSelector = frenzyFishId is not null
            ? i => new(Left: i * 8, Bottom: i % 2 * 16 - i % 3 * 4)
            : i => new(Left: i * 4, Bottom: i * 4);
        imagePanel.Children = Enumerable
            .Range(0, count)
            .Select(i =>
                new Image()
                {
                    Layout = LayoutParameters.FixedSize(64, 64),
                    Margin = marginSelector(i),
                    ZIndex = count - i,
                    Sprite = sprite,
                    Tint = new(Vector4.One * (1 - 0.08f * i)),
                    ShadowAlpha = 0.35f,
                    ShadowOffset = new(-2, 2),
                } as IView
            )
            .ToList();
    }
}
