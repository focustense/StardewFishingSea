using StardewUI;

namespace FishingBuddy.UI;

/// <summary>
/// Displays info about a seeded-random fish (e.g. jellies) that spawns in the location.
/// </summary>
internal class SeedFishInfoView : WrapperView
{
    /// <summary>
    /// The number of "other" fish that must be caught before the <see cref="FishId"/> becomes a
    /// possible result.
    /// </summary>
    public int CatchesRemaining
    {
        get => catchesRemaining;
        set => SetCatchesRemaining(value);
    }

    /// <summary>
    /// The fish/item that may be caught when <see cref="CatchesRemaining"/> reaches 0.
    /// </summary>
    public string? FishId
    {
        get => fishId;
        set => SetFishId(value);
    }

    private int catchesRemaining;
    private string? fishId;

    private Label? countdownLabel;
    private Panel? drawerPanel;
    private Image? fishPreviewImage;
    private Panel? fishPreviewPanel;
    private Lane? rowsLane;

    protected override IView CreateView()
    {
        var frameImage = new Image()
        {
            Layout = LayoutParameters.Fill(),
            Sprite = Sprites.OverlayFrame,
            Tint = new(150, 200, 240),
            Fit = ImageFit.Stretch,
        };
        fishPreviewImage = new Image()
        {
            Layout = LayoutParameters.FixedSize(64, 64),
            Margin = Sprites.OverlayFrame.FixedEdges! + new Edges(12),
        };
        fishPreviewPanel = new Panel()
        {
            Layout = LayoutParameters.FitContent(),
            Children = [frameImage, fishPreviewImage],
        };
        UpdateFishPreview();

        var drawerImage = new Image()
        {
            Layout = LayoutParameters.Fill(),
            Sprite = Sprites.OverlayDrawer,
            Fit = ImageFit.Stretch,
            Tint = new(150, 200, 240),
        };
        countdownLabel = new Label()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = new(Right: 4),
            Font = Game1.smallFont,
            MaxLines = 1,
        };
        var genericFishImage = new Image()
        {
            Layout = LayoutParameters.FitContent(),
            HorizontalAlignment = Alignment.Middle,
            VerticalAlignment = Alignment.Middle,
            Sprite = Sprites.GenericFish,
        };
        var countdownLane = new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = Sprites.OverlayDrawer.FixedEdges! + new Edges(8, 18, 24, 14),
            VerticalContentAlignment = Alignment.Middle,
            Children = [countdownLabel, genericFishImage],
        };
        drawerPanel = new Panel()
        {
            Layout = LayoutParameters.FitContent(),
            Children = [drawerImage, countdownLane],
        };
        rowsLane = new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            VerticalContentAlignment = Alignment.Middle,
            Children = [fishPreviewPanel],
        };
        UpdateCatchInfo();
        return rowsLane;
    }

    private void SetCatchesRemaining(int value)
    {
        if (catchesRemaining == value)
        {
            return;
        }
        catchesRemaining = value;
        UpdateCatchInfo();
    }

    private void SetFishId(string? value)
    {
        if (fishId == value)
        {
            return;
        }
        fishId = value;
        UpdateFishPreview();
    }

    private void UpdateCatchInfo()
    {
        if (countdownLabel is null || rowsLane is null)
        {
            return;
        }
        countdownLabel.Text = catchesRemaining.ToString();
        rowsLane.Children =
            catchesRemaining > 0 ? [fishPreviewPanel!, drawerPanel!] : [fishPreviewPanel!];
        if (fishPreviewImage is not null)
        {
            fishPreviewImage.Tint =
                catchesRemaining > 0 ? new(0.5f, 0.5f, 0.5f, 0.5f) : Color.White;
        }
    }

    private void UpdateFishPreview()
    {
        if (fishPreviewImage is null)
        {
            return;
        }
        fishPreviewImage.Sprite = fishId is not null ? Sprites.Item(fishId) : null;
    }
}
