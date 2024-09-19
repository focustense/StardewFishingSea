using FishinC.Data;
using StardewUI;

namespace FishinC.UI;

internal class RuleSetButton(string key, RuleSet ruleSet) : WrapperView
{
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            UpdateBackgroundTint();
        }
    }

    public string Key { get; } = key;

    public RuleSet RuleSet { get; } = ruleSet;

    private bool isSelected;
    private Frame? selectionFrame;

    protected override IView CreateView()
    {
        var icon = new Image()
        {
            Layout = LayoutParameters.FixedSize(80, 80),
            Margin = new(Bottom: 8),
            Sprite = Sprites.Item(RuleSet.SpriteItemId),
        };
        var label = Label.Simple(RuleSet.Title);
        var content = new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = Alignment.Middle,
            Children = [icon, label],
        };
        selectionFrame = new Frame()
        {
            Layout = new()
            {
                Width = Length.Content(),
                Height = Length.Content(),
                MinWidth = 120,
            },
            Padding = new(12),
            Background = new(Game1.staminaRect),
            BackgroundTint = Color.Transparent,
            HorizontalContentAlignment = Alignment.Middle,
            Content = content,
        };
        UpdateBackgroundTint();
        return new Frame()
        {
            Layout = LayoutParameters.FitContent(),
            Background = Sprites.ThinBorder,
            Margin = new(Right: 32),
            Padding = new(4),
            IsFocusable = true,
            Tooltip = RuleSet.Description,
            Content = selectionFrame,
        };
    }

    private void UpdateBackgroundTint()
    {
        if (selectionFrame is null)
        {
            return;
        }
        selectionFrame.BackgroundTint = IsSelected ? Color.LightBlue : Color.Transparent;
    }
}
