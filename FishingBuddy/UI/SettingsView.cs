﻿using FishingBuddy.Configuration;
using FishingBuddy.Data;
using StardewUI;

namespace FishingBuddy.UI;

/// <summary>
/// Mod configuration view; used in place of GMCM page.
/// </summary>
/// <param name="configContainer">Configuration container for the mod.</param>
internal class SettingsView(ModData data, IConfigurationContainer<ModConfig> configContainer)
    : WrapperView
{
    private readonly RuleSet customRuleSet =
        new()
        {
            Title = I18n.Settings_Difficulty_Custom_Title(),
            Description = I18n.Settings_Difficulty_Custom_Description(),
            SpriteItemId = "(O)128",
        };

    private readonly Lane ruleSetLane =
        new()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = new(Left: 16),
            Orientation = Orientation.Horizontal,
        };

    private readonly Lane rulesLane =
        new()
        {
            Layout = LayoutParameters.AutoRow(),
            Margin = new(Left: 16, Top: 16),
            Orientation = Orientation.Vertical,
        };

    private RuleSet selectedRuleSet = configContainer.Config.Rules.Clone();
    private string selectedRuleSetName = configContainer.Config.RuleSetName;

    // Initialized in CreateView
    private RulesForm rulesForm = null!;

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
        ruleSetLane.Children = data
            .RuleSets.Select(kv => new RuleSetButton(kv.Key, kv.Value))
            .Append(new("", customRuleSet))
            .Select(ruleSetButton =>
            {
                ruleSetButton.Click += RuleSetButton_Click;
                return ruleSetButton as IView;
            })
            .ToList();
        UpdateRuleSetButtons();
        rulesForm = new RulesForm(data, customRuleSet, 300);
        UpdateRules();
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
            Value = config.RespawnInterval,
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
                FormBuilder.CreateSectionHeading(I18n.Settings_Difficulty_Heading()),
                ruleSetLane,
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

    private IView? CreateRuleRow(string featureTitle, string conditionName, bool asVisibility)
    {
        if (!data.FeatureConditions.TryGetValue(conditionName, out var condition))
        {
            return null;
        }
        var text = asVisibility
            ? I18n.Settings_Rules_RuleTemplate_Visibility(
                featureTitle,
                condition.VisibilityRuleText
            )
            : I18n.Settings_Rules_RuleTemplate_Required(
                featureTitle,
                condition.RequirementRuleText
            );
        return CreateBulletPoint(text);
    }

    private void RuleSetButton_Click(object? sender, ClickEventArgs e)
    {
        if (sender is not RuleSetButton button || button.IsSelected)
        {
            return;
        }
        Game1.playSound("drumkit6");
        selectedRuleSetName = button.Key;
        selectedRuleSet = button.RuleSet;
        UpdateRuleSetButtons();
        UpdateRules();
    }

    private void UpdateReadOnlyRules(RuleSet ruleSet)
    {
        var currentBubblesRow = CreateRuleRow(
            I18n.Settings_Rules_CurrentBubbles_Title(),
            ruleSet.CurrentBubbles,
            true
        );
        var futureBubblesRow = CreateRuleRow(
            I18n.Settings_Rules_FutureBubbles_Title(),
            ruleSet.FutureBubbles,
            true
        );
        var fishPredictionsRow = CreateRuleRow(
            I18n.Settings_Rules_FishPredictions_Title(),
            ruleSet.FishPredictions,
            false
        );
        var jellyPredictionsRow = CreateRuleRow(
            I18n.Settings_Rules_JellyPredictions_Title(),
            ruleSet.JellyPredictions,
            false
        );
        var freezeRow = CreateBulletPoint(
            ruleSet.FreezeOnCast
                ? I18n.Settings_Rules_FreezeOnCast_Enabled()
                : I18n.Settines_Rules_FreezeOnCast_Disabled()
        );
        var respawnRow = CreateBulletPoint(
            ruleSet.RespawnOnCancel
                ? I18n.Settings_Rules_RespawnOnCancel_Enabled()
                : I18n.Settings_Rules_RespawnOnCancel_Disabled()
        );
        IView?[] allRows =
        [
            currentBubblesRow,
            futureBubblesRow,
            fishPredictionsRow,
            jellyPredictionsRow,
            freezeRow,
            respawnRow,
        ];
        rulesLane.Children = allRows.Where(row => row is not null).Select(row => row!).ToList();
    }

    private void UpdateRules()
    {
        if (selectedRuleSet == customRuleSet)
        {
            rulesLane.Children = [rulesForm];
        }
        else
        {
            UpdateReadOnlyRules(selectedRuleSet);
        }
    }

    private void UpdateRuleSetButtons()
    {
        foreach (var ruleSetButton in ruleSetLane.Children.OfType<RuleSetButton>())
        {
            ruleSetButton.IsSelected = ruleSetButton.Key == selectedRuleSetName;
        }
    }
}
