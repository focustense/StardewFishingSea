using StardewUI;

namespace FishingBuddy.UI;

/// <summary>
/// Mod configuration view; used in place of GMCM page.
/// </summary>
/// <param name="configContainer">Configuration container for the mod.</param>
internal class SettingsView(IConfigurationContainer<ModConfig> configContainer) : WrapperView
{
    protected override IView CreateView()
    {
        var config = configContainer.Config;
        var header = CreateHeader();
        var separator = new Image()
        {
            Layout = LayoutParameters.AutoRow(),
            Margin = UiSprites.MenuHorizontalDividerMargin,
            Fit = ImageFit.Stretch,
            Sprite = UiSprites.MenuHorizontalDivider,
        };
        var descriptionLabel = new Label()
        {
            Layout = new() { Width = Length.Percent(100), Height = Length.Content() },
            Text = I18n.Settings_ModDescription(),
            Color = Colors.MutedText,
        };
        var difficultyLane = new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = new(Left: 16),
            Orientation = Orientation.Horizontal,
            Children =
            [
                CreateDifficultyButton(
                    I18n.Settings_Difficulty_Easy_Title(),
                    I18n.Settings_Difficulty_Easy_Description(),
                    "(O)142"
                ),
                CreateDifficultyButton(
                    I18n.Settings_Difficulty_Medium_Title(),
                    I18n.Settings_Difficulty_Medium_Description(),
                    "(O)143"
                ),
                CreateDifficultyButton(
                    I18n.Settings_Difficulty_Hard_Title(),
                    I18n.Settings_Difficulty_Hard_Description(),
                    "(O)163"
                ),
                CreateDifficultyButton(
                    I18n.Settings_Difficulty_Custom_Title(),
                    I18n.Settings_Difficulty_Custom_Description(),
                    "(O)128"
                ),
            ],
        };
        var rulesLane = new Lane()
        {
            Layout = LayoutParameters.AutoRow(),
            Margin = new(Left: 16, Top: 16),
            Orientation = Orientation.Vertical,
            Children =
            [
                CreateBulletPoint("Bubble durations are visible with Fisher profession"),
                CreateBulletPoint("Upcoming bubbles are visible with Angler profession"),
                CreateBulletPoint("Fish predictions require equipped Sonar Bobber"),
                CreateBulletPoint("Jelly predictions require equipped Sonar Bobber"),
                CreateBulletPoint("Outcomes cannot change while the line is cast"),
                CreateBulletPoint("Fish are not rerolled after a cancelled cast"),
            ],
        };
        var speedupSlider = new Slider()
        {
            TrackWidth = 300,
            Min = 1f,
            Max = 20f,
            Interval = 1f,
            Value = config.FishingTimeScale,
            ValueColor = Colors.MutedText,
            ValueFormat = v => I18n.Settings_Time_FishingSpeedup_ValueFormat(v),
        };
        var spawnIntervalSlider = new Slider()
        {
            TrackWidth = 300,
            Min = 10f,
            Max = 300f,
            Interval = 10f,
            Value = config.CatchUpdateInterval,
            ValueColor = Colors.MutedText,
            ValueFormat = v => I18n.Settings_Time_RerollInterval_ValueFormat((int)v),
        };
        var hudLocationChooser = new ScreenLocationChooser()
        {
            Corner = config.SeededRandomFishHudLocation,
            Offset = config.SeededRandomFishHudOffset.ToVector2(),
        };
        var form = new FormBuilder(300)
            .AddSection(I18n.Settings_Time_Heading())
            .AddField(
                I18n.Settings_Time_FishingSpeedup_Title(),
                I18n.Settings_Time_FishingSpeedup_Description(),
                speedupSlider
            )
            .AddField(
                I18n.Settings_Time_RerollInterval_Title(),
                I18n.Settings_Time_RerollInterval_Description(),
                spawnIntervalSlider
            )
            .AddSection(I18n.Settings_UI_Heading())
            .AddField(
                I18n.Settings_UI_HudLocation_Title(),
                I18n.Settings_UI_HudLocation_Description(),
                hudLocationChooser
            )
            .Build();
        var mainContent = new Lane()
        {
            Layout = LayoutParameters.AutoRow(),
            Orientation = Orientation.Vertical,
            Padding = new(8),
            Children =
            [
                descriptionLabel,
                CreateSectionHeading(I18n.Settings_Difficulty_Heading()),
                difficultyLane,
                rulesLane,
                form,
            ],
        };
        var flow = new Lane()
        {
            Layout = LayoutParameters.AutoRow(),
            Orientation = Orientation.Vertical,
            Children = [header, separator, mainContent],
        };
        return new ScrollableFrameView()
        {
            FrameLayout = LayoutParameters.FixedSize(800, 1080),
            Content = flow,
        };
    }

    private IView CreateBulletPoint(string text)
    {
        var bullet = new Image()
        {
            Layout = LayoutParameters.FixedSize(21, 24),
            Margin = new(Right: 8),
            Sprite = Sprites.BobberDefault,
            Tint = new(0.8f, 0.8f, 0.8f, 0.8f),
        };
        var label = new Label()
        {
            Layout = LayoutParameters.AutoRow(),
            Color = Colors.MutedText,
            Text = text,
        };
        return new Lane()
        {
            Layout = LayoutParameters.AutoRow(),
            Margin = new(Bottom: 8),
            VerticalContentAlignment = Alignment.Middle,
            Children = [bullet, label],
        };
    }

    private IView CreateDifficultyButton(string title, string description, string iconItemId)
    {
        var icon = new Image()
        {
            Layout = LayoutParameters.FixedSize(80, 80),
            Margin = new(Bottom: 8),
            Sprite = Sprites.Item(iconItemId),
        };
        var label = Label.Simple(title);
        var content = new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = Alignment.Middle,
            Children = [icon, label],
        };
        var backgroundTint = iconItemId == "(O)143" ? Color.LightBlue : Color.Transparent;
        var selectionFrame = new Frame()
        {
            Layout = new()
            {
                Width = Length.Content(),
                Height = Length.Content(),
                MinWidth = 120,
            },
            Padding = new(12),
            Background = new(Game1.staminaRect),
            BackgroundTint = backgroundTint,
            HorizontalContentAlignment = Alignment.Middle,
            Content = content,
        };
        return new Frame()
        {
            Layout = LayoutParameters.FitContent(),
            Background = Sprites.ThinBorder,
            Margin = new(Right: 32),
            Padding = new(4),
            IsFocusable = true,
            Tooltip = description,
            Content = selectionFrame,
        };
    }

    private IView CreateHeader()
    {
        var background = new Image()
        {
            Layout = LayoutParameters.Fill(),
            Fit = ImageFit.Cover,
            Sprite = Sprites.WaterBackground,
        };
        var title = new Banner()
        {
            Layout = LayoutParameters.FitContent(),
            Text = I18n.Settings_ModTitle(),
            TextShadowAlpha = 0.6f,
            TextShadowOffset = new(-4, 4),
        };
        var giantBaitImage = new Image()
        {
            Layout = LayoutParameters.FixedSize(128, 128),
            Margin = new(Left: 32),
        };
        _ = new SpriteAnimator(giantBaitImage)
        {
            FrameDuration = TimeSpan.FromMilliseconds(500),
            Frames = Sprites.GiantBait,
        };
        var mermaidImage = new Image()
        {
            Layout = LayoutParameters.FixedSize(128, 128),
            Margin = new(Right: 24),
        };
        _ = new SpriteAnimator(mermaidImage)
        {
            FrameDuration = TimeSpan.FromMilliseconds(150),
            Frames =
            [
                Sprites.Mermaid[0],
                Sprites.Mermaid[1],
                Sprites.Mermaid[2],
                Sprites.Mermaid[1],
                Sprites.Mermaid[2],
                Sprites.Mermaid[3],
                Sprites.Mermaid[4],
            ],
            StartDelay = TimeSpan.FromSeconds(4),
        };
        return new Panel()
        {
            Layout = new() { Width = Length.Stretch(), Height = Length.Px(150) },
            HorizontalContentAlignment = Alignment.Middle,
            VerticalContentAlignment = Alignment.Middle,
            Children =
            [
                background,
                title,
                Panel.Align(giantBaitImage, Alignment.Start, Alignment.Middle),
                Panel.Align(mermaidImage, Alignment.End, Alignment.Middle),
            ],
        };
    }

    private IView CreateSectionHeading(string text)
    {
        return new Banner() { Margin = new(Top: 16, Bottom: 8), Text = text };
    }
}
