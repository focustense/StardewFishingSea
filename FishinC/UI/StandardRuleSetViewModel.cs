using System.ComponentModel;
using FishinC.Data;
using PropertyChanged.SourceGenerator;
using StardewValley.ItemTypeDefinitions;

namespace FishinC.UI;

/// <summary>
/// View model for displaying the rules of a standard, read-only rule set.
/// </summary>
/// <param name="data">The current mod data.</param>
/// <param name="name">Name (identifying key) of the rule set.</param>
/// <param name="ruleSet">The detailed rules.</param>
public partial class StandardRuleSetViewModel(ModData data, string name, RuleSet ruleSet)
    : INotifyPropertyChanged
{
    /// <inheritdoc cref="RuleSet.Description" />
    public string Description { get; } = ruleSet.Description;

    /// <summary>
    /// Name (identifying key) of the rule set.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The detailed rules.
    /// </summary>
    public RuleSet RuleSet { get; } = ruleSet;

    /// <summary>
    /// List of localized, formatted strings, each explaining one rule.
    /// </summary>
    public IReadOnlyList<string> Rules { get; } = FormatRules(data, ruleSet);

    /// <summary>
    /// Sprite data for this rule set's <see cref="RuleSet.SpriteItemId"/>.
    /// </summary>
    public ParsedItemData SpriteData { get; } =
        ItemRegistry.GetDataOrErrorItem(ruleSet.SpriteItemId);

    /// <inheritdoc cref="RuleSet.Title" />
    public string Title { get; } = ruleSet.Title;

    /// <summary>
    /// Whether this is the currently-selected rule set in the overall settings UI.
    /// </summary>
    [Notify]
    private bool isSelected;

    private static string? FormatRule(
        ModData data,
        string featureTitle,
        string conditionName,
        bool asVisibility
    )
    {
        if (!data.FeatureConditions.TryGetValue(conditionName, out var condition))
        {
            return null;
        }
        return asVisibility
            ? I18n.Settings_Rules_RuleTemplate_Visibility(
                featureTitle,
                condition.VisibilityRuleText
            )
            : I18n.Settings_Rules_RuleTemplate_Required(
                featureTitle,
                condition.RequirementRuleText
            );
    }

    private static IReadOnlyList<string> FormatRules(ModData data, RuleSet ruleSet)
    {
        var currentBubblesRule = FormatRule(
            data,
            I18n.Settings_Rules_CurrentBubbles_Title(),
            ruleSet.CurrentBubbles,
            true
        );
        var futureBubblesRule = FormatRule(
            data,
            I18n.Settings_Rules_FutureBubbles_Title(),
            ruleSet.FutureBubbles,
            true
        );
        var fishPredictionsRule = FormatRule(
            data,
            I18n.Settings_Rules_FishPredictions_Title(),
            ruleSet.FishPredictions,
            false
        );
        var jellyPredictionsRule = FormatRule(
            data,
            I18n.Settings_Rules_JellyPredictions_Title(),
            ruleSet.JellyPredictions,
            false
        );
        var freezeRule = ruleSet.FreezeOnCast
            ? I18n.Settings_Rules_FreezeOnCast_Enabled()
            : I18n.Settings_Rules_FreezeOnCast_Disabled();
        var respawnRule = ruleSet.RespawnOnCancel
            ? I18n.Settings_Rules_RespawnOnCancel_Enabled()
            : I18n.Settings_Rules_RespawnOnCancel_Disabled();
        return new[]
        {
            currentBubblesRule,
            futureBubblesRule,
            fishPredictionsRule,
            jellyPredictionsRule,
            freezeRule,
            respawnRule,
        }
            .Where(s => !string.IsNullOrEmpty(s))
            .Cast<string>()
            .ToList();
    }
}
