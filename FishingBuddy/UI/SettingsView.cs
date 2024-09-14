using FishingBuddy.Configuration;
using FishingBuddy.Data;
using StardewUI;
using StardewUI.Widgets.Keybinding;

namespace FishingBuddy.UI;

/// <summary>
/// Mod configuration view; used in place of GMCM page.
/// </summary>
/// <param name="configContainer">Configuration container for the mod.</param>
internal class SettingsView(ModData data, IConfigurationContainer<ModConfig> configContainer)
    : WrapperView
{
    private const int SLIDER_WIDTH = 300;

    private ModConfig Config => configContainer.Config;

    private readonly RuleSet customRuleSet =
        new()
        {
            Title = I18n.Settings_Difficulty_Custom_Title(),
            Description = I18n.Settings_Difficulty_Custom_Description(),
            SpriteItemId = "(O)128",
        };

    private readonly KeybindListEditor keybindListEditor =
        new(data.GetButtonSpriteMap())
        {
            ButtonHeight = 48,
            Font = Game1.smallFont,
            EditableType = KeybindType.MultipleKeybinds,
            AddButtonText = I18n.Settings_UI_PreviewKeybind_Editor_Add_Text(),
            DeleteButtonTooltip = I18n.Settings_UI_PreviewKeybind_Editor_Delete_Description(),
            EmptyText = I18n.Settings_UI_PreviewKeybind_Empty(),
            EmptyTextColor = Colors.MutedText,
            KeybindList = configContainer.Config.CatchPreviewToggleKeybind,
            IsFocusable = true,
        };

    private readonly Slider previewRadiusSlider =
        new()
        {
            TrackWidth = SLIDER_WIDTH,
            Min = 2,
            Max = 20,
            Interval = 1f,
            Value = configContainer.Config.CatchPreviewTileRadius,
            ValueColor = Colors.MutedText,
            ValueFormat = v => I18n.Settings_UI_CatchPreviewRadius_ValueFormat(v),
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

    private readonly Slider spawnIntervalSlider =
        new()
        {
            TrackWidth = SLIDER_WIDTH,
            Min = 10f,
            Max = 300f,
            Interval = 10f,
            Value = configContainer.Config.RespawnInterval,
            ValueColor = Colors.MutedText,
            ValueFormat = v => I18n.Settings_Time_RerollInterval_ValueFormat((int)v),
        };

    private readonly Slider speedupSlider =
        new()
        {
            TrackWidth = SLIDER_WIDTH,
            Min = 1f,
            Max = 20f,
            Interval = 1f,
            Value = configContainer.Config.FishingTimeScale,
            ValueColor = Colors.MutedText,
            ValueFormat = v => I18n.Settings_Time_FishingSpeedup_ValueFormat(v),
        };

    private RuleSet selectedRuleSet = configContainer.Config.Rules.Clone();
    private string selectedRuleSetName = configContainer.Config.RuleSetName;

    // Initialized in CreateView
    private RulesForm rulesForm = null!;

    protected override IView CreateView()
    {
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
                ruleSetButton.LeftClick += RuleSetButton_LeftClick;
                return ruleSetButton as IView;
            })
            .ToList();
        UpdateRuleSetButtons();
        if (string.IsNullOrEmpty(selectedRuleSetName))
        {
            customRuleSet.CopyFrom(selectedRuleSet);
            selectedRuleSet = customRuleSet;
        }
        rulesForm = new RulesForm(data, customRuleSet, 300);
        UpdateRules();
        var hudLocationChooser = new ScreenLocationChooser()
        {
            Corner = Config.SeededRandomFishHudLocation,
            Offset = Config.SeededRandomFishHudOffset.ToVector2(),
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
                I18n.Settings_UI_PreviewKeybind_Title(),
                I18n.Settings_UI_PreviewKeybind_Description(),
                keybindListEditor
            )
            .AddField(
                I18n.Settings_UI_CatchPreviewRadius_Title(),
                I18n.Settings_UI_CatchPreviewRadius_Description(),
                previewRadiusSlider
            )
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
        var mainContentScrollable = new ScrollableView()
        {
            Layout = LayoutParameters.Fill(),
            Peeking = 128,
            Content = mainContent,
        };
        return new Frame()
        {
            Background = UiSprites.MenuBackground,
            Border = UiSprites.MenuBorder,
            BorderThickness = UiSprites.MenuBorderThickness,
            Content = new Lane()
            {
                Layout = LayoutParameters.FixedSize(800, 950),
                Orientation = Orientation.Vertical,
                Children = [header, separator, mainContentScrollable],
            },
            FloatingElements =
            [
                new(
                    new Lane()
                    {
                        Layout = LayoutParameters.AutoRow(),
                        Margin = new(Right: 20, Top: -12),
                        HorizontalContentAlignment = Alignment.End,
                        VerticalContentAlignment = Alignment.Middle,
                        Children =
                        [
                            CreateActionButton(I18n.Settings_Button_Default(), ResetConfig),
                            CreateActionButton(I18n.Settings_Button_Cancel(), Close),
                            CreateActionButton(I18n.Settings_Button_Save(), SaveConfigAndClose),
                        ],
                    },
                    FloatingPosition.BelowParent
                ),
            ],
        };
    }

    private void Close()
    {
        Game1.playSound("bigDeSelect");
        Game1.activeClickableMenu = null;
    }

    private Button CreateActionButton(string text, Action onLeftClick)
    {
        var button = new Button(UiSprites.ButtonDark, UiSprites.ButtonLight)
        {
            Layout = new()
            {
                Width = Length.Content(),
                Height = Length.Content(),
                MinWidth = 150,
            },
            Margin = new(Left: 8),
            Font = Game1.dialogueFont,
            Text = text,
            ShadowVisible = true,
        };
        button.LeftClick += (_, _) => onLeftClick();
        return button;
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
            Sprite = Sprites.GiantBait[0],
        };
        var mermaidImage = new Image()
        {
            Layout = LayoutParameters.FixedSize(128, 128),
            Margin = new(Right: 24),
            Sprite = Sprites.Mermaid[0],
        };
        StartAnimation(giantBaitImage, mermaidImage);
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

    private void ResetConfig()
    {
        Game1.playSound("drumkit6");
        var defaultConfig = configContainer.GetDefault();
        UpdateFromConfig(defaultConfig);
    }

    private void RuleSetButton_LeftClick(object? sender, ClickEventArgs e)
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

    private void SaveConfigAndClose()
    {
        Config.RuleSetName = selectedRuleSetName;
        Config.Rules = selectedRuleSet.Clone();
        Config.FishingTimeScale = speedupSlider.Value;
        Config.RespawnInterval = (int)spawnIntervalSlider.Value;
        Config.CatchPreviewToggleKeybind = keybindListEditor.KeybindList;
        Config.CatchPreviewTileRadius = (int)previewRadiusSlider.Value;
        configContainer.Save();
        Close();
    }

    private static void StartAnimation(Image giantBaitImage, Image mermaidImage)
    {
        _ = new SpriteAnimator(giantBaitImage)
        {
            FrameDuration = TimeSpan.FromMilliseconds(500),
            Frames = Sprites.GiantBait,
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
    }

    private void UpdateFromConfig(ModConfig config)
    {
        selectedRuleSetName = config.RuleSetName;
        selectedRuleSet = config.Rules.Clone();
        speedupSlider.Value = config.FishingTimeScale;
        spawnIntervalSlider.Value = config.RespawnInterval;
        keybindListEditor.KeybindList = config.CatchPreviewToggleKeybind;
        previewRadiusSlider.Value = config.CatchPreviewTileRadius;
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
