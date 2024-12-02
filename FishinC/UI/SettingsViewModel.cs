using System.ComponentModel;
using FishinC.Configuration;
using FishinC.Data;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI.Utilities;

namespace FishinC.UI;

/// <summary>
/// View model for the mod settings menu.
/// </summary>
/// <param name="configContainer">Configuration container for the mod.</param>
/// <param name="data">Current mod data.</param>
/// <param name="close">Action to close the containing menu.</param>
public partial class SettingsViewModel(
    IConfigurationContainer<ModConfig> configContainer,
    ModData data,
    Action close
) : INotifyPropertyChanged
{
    /// <summary>
    /// The current custom rules. These persist when switching to a standard rule set until the menu
    /// is closed.
    /// </summary>
    public CustomRuleSetViewModel CustomRuleSet { get; } =
        GetCustomRuleSet(configContainer.Config, data);

    /// <summary>
    /// Info to show as an example in the HUD position editor.
    /// </summary>
    public SeedFishInfoViewModel ExampleSeedFishInfo { get; } =
        new() { FishId = "(O)CaveJelly", CatchesRemaining = 13 };

    /// <summary>
    /// Function that accepts the <see cref="ModConfig.FishingTimeScale"/> value and returns a
    /// formatted display string.
    /// </summary>
    public Func<float, string> FishingTimeScaleFormat { get; } =
        v => I18n.Settings_Time_FishingSpeedup_ValueFormat(v);

    /// <summary>
    /// State of the menu's header.
    /// </summary>
    public SettingsHeaderViewModel Header { get; } = new();

    /// <summary>
    /// Whether the <see cref="CustomRuleSet"/> is the one currently selected.
    /// </summary>
    /// <remarks>
    /// Corresponds to <see cref="SelectedRuleSetType"/> having a value of <c>custom</c>.
    /// </remarks>
    public bool IsCustomRuleSetSelected
    {
        get => SelectedStandardRuleSet is null;
        set
        {
            if (value && !IsCustomRuleSetSelected)
            {
                SelectedRuleSetName = "";
            }
        }
    }

    /// <summary>
    /// Function that accepts the <see cref="ModConfig.CatchPreviewTileRadius"/> value and returns a
    /// formatted display string.
    /// </summary>
    public Func<float, string> PreviewRadiusFormat { get; } =
        v => I18n.Settings_UI_CatchPreviewRadius_ValueFormat(v);

    /// <summary>
    /// Flavor quote to display directly below the header.
    /// </summary>
    public string Quote { get; } = Quotes.GetRandomQuote();

    /// <summary>
    /// Function that accepts the <see cref="ModConfig.RespawnInterval"/> value and returns a
    /// formatted display string.
    /// </summary>
    public Func<float, string> RespawnIntervalFormat { get; } =
        v => I18n.Settings_Time_RerollInterval_ValueFormat(v);

    /// <summary>
    /// The standard rule set that is currently selected, if any.
    /// </summary>
    public StandardRuleSetViewModel? SelectedStandardRuleSet =>
        !string.IsNullOrEmpty(SelectedRuleSetName)
        && standardRuleSets.TryGetValue(SelectedRuleSetName, out var ruleSet)
            ? ruleSet
            : null;

    /// <summary>
    /// The type of rule set currently selected (standard or custom).
    /// </summary>
    public string SelectedRuleSetType =>
        SelectedStandardRuleSet is not null ? "standard" : "custom";

    /// <summary>
    /// All available standard rule sets.
    /// </summary>
    public IEnumerable<StandardRuleSetViewModel> StandardRuleSets => standardRuleSets.Values;

    /// <summary>
    /// The current mod configuration.
    /// </summary>
    private ModConfig Config => configContainer.Config;

    private readonly Dictionary<string, StandardRuleSetViewModel> standardRuleSets =
        data.RuleSets.ToDictionary(
            x => x.Key,
            x => new StandardRuleSetViewModel(data, x.Key, x.Value)
            {
                IsSelected = x.Key.Equals(
                    configContainer.Config.RuleSetName,
                    StringComparison.CurrentCultureIgnoreCase
                ),
            }
        );

    /// <inheritdoc cref="ModConfig.CatchPreviewToggleKeybind"/>
    [Notify]
    private KeybindList catchPreviewToggleKeybinds = configContainer
        .Config
        .CatchPreviewToggleKeybind;

    /// <inheritdoc cref="ModConfig.EnablePreviewsOnLoad"/>
    [Notify]
    private bool enablePreviewsOnLoad = configContainer.Config.EnablePreviewsOnLoad;

    /// <inheritdoc cref="ModConfig.FishingTimeScale"/>
    [Notify]
    private float fishingTimeScale = configContainer.Config.FishingTimeScale;

    /// <inheritdoc cref="ModConfig.CatchPreviewTileRadius"/>
    [Notify]
    private float previewRadius = configContainer.Config.CatchPreviewTileRadius;

    /// <inheritdoc cref="ModConfig.RespawnInterval"/>
    [Notify]
    private float respawnInterval = configContainer.Config.RespawnInterval;

    /// <inheritdoc cref="ModConfig.SeededRandomFishHudPlacement"/>
    [Notify]
    private NineGridPlacement seededRandomFishHudPlacement = configContainer
        .Config
        .SeededRandomFishHudPlacement;

    /// <inheritdoc cref="ModConfig.RuleSetName"/>
    [Notify]
    private string selectedRuleSetName = configContainer.Config.RuleSetName;

    /// <summary>
    /// Closes the menu.
    /// </summary>
    /// <param name="save">Whether to save the settings before closing.</param>
    public void Close(bool save)
    {
        if (save)
        {
            Save();
        }
        // This bypasses the menu's own close logic, so we have to play the sound manually.
        Game1.playSound("bigDeSelect");
        close();
    }

    /// <summary>
    /// Performs the action associated with an action button.
    /// </summary>
    /// <remarks>
    /// This is a temporary workaround for StardewUI Templates not supporting substitution of entire
    /// event handlers (only their arguments).
    /// </remarks>
    /// <param name="actionName"></param>
    public void PerformAction(string actionName)
    {
        switch (actionName)
        {
            case "reset":
                Reset();
                break;
            case "cancel":
                Close(false);
                break;
            case "save":
                Close(true);
                break;
        }
    }

    /// <summary>
    /// Resets all configuration settings to their default values for the mod.
    /// </summary>
    public void Reset()
    {
        Game1.playSound("drumkit6");
        var config = configContainer.GetDefault();
        SelectedRuleSetName = config.RuleSetName;
        foreach (var ruleSet in standardRuleSets.Values)
        {
            ruleSet.IsSelected = ruleSet.Name.Equals(
                SelectedRuleSetName,
                StringComparison.CurrentCultureIgnoreCase
            );
        }
        CustomRuleSet.Reset(config.Rules);
        FishingTimeScale = config.FishingTimeScale;
        RespawnInterval = config.RespawnInterval;
        EnablePreviewsOnLoad = config.EnablePreviewsOnLoad;
        CatchPreviewToggleKeybinds = config.CatchPreviewToggleKeybind;
        PreviewRadius = config.CatchPreviewTileRadius;
        SeededRandomFishHudPlacement = config.SeededRandomFishHudPlacement;
    }

    /// <summary>
    /// Saves the settings currently displayed in the UI to the mod configuration.
    /// </summary>
    public void Save()
    {
        Config.RuleSetName = SelectedRuleSetName;
        Config.Rules = CustomRuleSet.ToConfig();
        Config.FishingTimeScale = (int)FishingTimeScale;
        Config.RespawnInterval = (int)RespawnInterval;
        Config.EnablePreviewsOnLoad = EnablePreviewsOnLoad;
        Config.CatchPreviewToggleKeybind = CatchPreviewToggleKeybinds;
        Config.CatchPreviewTileRadius = (int)PreviewRadius;
        Config.SeededRandomFishHudPlacement = SeededRandomFishHudPlacement;
        configContainer.Save();
    }

    /// <summary>
    /// Changes the selected rule set.
    /// </summary>
    /// <param name="name">Name of a standard rule set, or empty to use custom rules.</param>
    public void SelectRuleSet(string name)
    {
        if (
            name.Equals(SelectedRuleSetName, StringComparison.CurrentCultureIgnoreCase)
            || (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(SelectedRuleSetName))
        )
        {
            return;
        }
        Game1.playSound("drumkit6");
        SelectedRuleSetName = name;
        foreach (var ruleSet in standardRuleSets.Values)
        {
            ruleSet.IsSelected = ruleSet.Name.Equals(
                name,
                StringComparison.CurrentCultureIgnoreCase
            );
        }
    }

    private static CustomRuleSetViewModel GetCustomRuleSet(ModConfig config, ModData data)
    {
        var ruleSet = new RuleSet()
        {
            Title = I18n.Settings_Difficulty_Custom_Title(),
            Description = I18n.Settings_Difficulty_Custom_Description(),
            SpriteItemId = "(O)128",
        };
        if (string.IsNullOrEmpty(config.RuleSetName))
        {
            ruleSet.CopyFrom(config.Rules);
        }
        return new(data, ruleSet);
    }
}
