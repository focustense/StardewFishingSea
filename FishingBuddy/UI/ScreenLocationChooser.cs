using FishingBuddy.Configuration;
using StardewUI;
using StardewUI.Widgets;
using StardewValley.TerrainFeatures;

namespace FishingBuddy.UI;

/// <summary>
/// Widget for choosing a screen corner and offset.
/// </summary>
public class ScreenLocationChooser(
    ISpriteMap<SButton> buttonSpriteMap,
    ISpriteMap<Direction> directionSpriteMap
) : WrapperView
{
    public RectangleCorner Corner
    {
        get => corner;
        set
        {
            if (value == corner)
            {
                return;
            }
            corner = value;
            UpdatePlacementFrames();
        }
    }

    public Vector2 Offset
    {
        get => offset;
        set
        {
            if (value == offset)
            {
                return;
            }
            offset = value;
            UpdatePlacementFrames();
        }
    }

    private RectangleCorner corner;
    private Vector2 offset;
    private Frame? bottomLeftFrame;
    private Frame? bottomRightFrame;
    private Frame? topLeftFrame;
    private Frame? topRightFrame;

    protected override IView CreateView()
    {
        var background = new Image()
        {
            Layout = LayoutParameters.FixedSize(128, 128),
            Sprite = Sprites.Item("(F)1468"),
        };
        topLeftFrame = CreatePlacementFrame(RectangleCorner.TopLeft);
        topRightFrame = CreatePlacementFrame(RectangleCorner.TopRight);
        bottomLeftFrame = CreatePlacementFrame(RectangleCorner.BottomLeft);
        bottomRightFrame = CreatePlacementFrame(RectangleCorner.BottomRight);
        UpdatePlacementFrames();
        var panel = new Panel()
        {
            Layout = LayoutParameters.FitContent(),
            Children = [background, topLeftFrame, topRightFrame, bottomLeftFrame, bottomRightFrame],
            IsFocusable = true,
        };
        panel.LeftClick += Panel_LeftClick;
        return panel;
    }

    private void Panel_LeftClick(object? sender, ClickEventArgs e)
    {
        Overlay.Push(
            new PositioningOverlay(buttonSpriteMap, directionSpriteMap)
            {
                Content = new SeedFishInfoView() { FishId = "(O)CaveJelly", CatchesRemaining = 13 },
                DimmingAmount = 0.93f,
                ContentPlacement = new(Alignment.Middle, Alignment.Start),
            }
        );
    }

    private static Frame CreatePlacementFrame(RectangleCorner corner)
    {
        var placementOverlay = new Image()
        {
            Layout = LayoutParameters.FixedSize(32, 32),
            Sprite = new(Game1.staminaRect),
        };
        return new Frame()
        {
            Layout = LayoutParameters.Fill(),
            Margin = new(8, 21, 8, 32),
            HorizontalContentAlignment = corner switch
            {
                RectangleCorner.TopRight or RectangleCorner.BottomRight => Alignment.End,
                _ => Alignment.Start,
            },
            VerticalContentAlignment = corner switch
            {
                RectangleCorner.BottomLeft or RectangleCorner.BottomRight => Alignment.End,
                _ => Alignment.Start,
            },
            Content = placementOverlay,
        };
    }

    private static void UpdatePlacementFrame(Frame? placementFrame, bool isActive)
    {
        if (placementFrame?.Content is not Image image)
        {
            return;
        }
        image.Tint = isActive ? new(0.2f, 0.8f, 0f, 0.5f) : new(0.25f, 0.15f, 0.05f, 0.15f);
    }

    private void UpdatePlacementFrames()
    {
        UpdatePlacementFrame(topLeftFrame, Corner == RectangleCorner.TopLeft);
        UpdatePlacementFrame(topRightFrame, Corner == RectangleCorner.TopRight);
        UpdatePlacementFrame(bottomLeftFrame, Corner == RectangleCorner.BottomLeft);
        UpdatePlacementFrame(bottomRightFrame, Corner == RectangleCorner.BottomRight);
    }
}
