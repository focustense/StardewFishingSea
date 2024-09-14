using Microsoft.Xna.Framework.Graphics;
using StardewUI;
using StardewUI.Widgets.Keybinding;

namespace FishingBuddy.Data;

/// <summary>
/// Holds all built-in/moddable data for the mod.
/// </summary>
internal class ModData(IModHelper helper)
{
    /// <summary>
    /// Dictionary of condition names to feature conditions.
    /// </summary>
    public Dictionary<string, FeatureCondition> FeatureConditions => featureConditions.Asset;

    /// <summary>
    /// Sprite sheet containing the sprites for gamepad buttons.
    /// </summary>
    public Texture2D KeybindButtons => keybindButtonsTexture.Asset;

    /// <summary>
    /// Sprite sheet containing the sprites used for keyboard keys.
    /// </summary>
    public Texture2D KeybindKeys => keybindKeysTexture.Asset;

    /// <summary>
    /// Dictionary of built-in rule set names to their rules.
    /// </summary>
    public Dictionary<string, RuleSet> RuleSets => ruleSets.Asset;

    private readonly LazyAsset<Dictionary<string, FeatureCondition>> featureConditions =
        new(
            helper,
            $"Mods/{helper.ModContent.ModID}/Conditions",
            "assets/conditions.json",
            OnLoadFeatureConditions
        );

    private readonly LazyAsset<Texture2D> keybindButtonsTexture =
        new(helper, $"Mods/{helper.ModContent.ModID}/KeybindButtons", "assets/XboxButtons.png");

    private readonly LazyAsset<Texture2D> keybindKeysTexture =
        new(helper, $"Mods/{helper.ModContent.ModID}/KeybindKeys", "assets/KeyboardKeys.png");

    private readonly LazyAsset<Dictionary<string, RuleSet>> ruleSets =
        new(helper, $"Mods/{helper.ModContent.ModID}/Rules", "assets/rules.json", OnLoadRuleSets);

    /// <summary>
    /// Gets the sprite map for gamepad buttons/keyboard keys used in keybinds/button prompts.
    /// </summary>
    public ISpriteMap<SButton> GetButtonSpriteMap()
    {
        return new XeluButtonSpriteMap(KeybindButtons, KeybindKeys);
    }

    private static void OnLoadFeatureConditions(
        Dictionary<string, FeatureCondition> featureConditions
    )
    {
        foreach (var (name, condition) in featureConditions)
        {
            condition.Title = I18n.GetByKey($"BuiltinConditions.{name}.Title").Default(name);
            condition.RequirementRuleText = I18n.GetByKey(
                    $"BuiltinConditions.{name}.RequirementRuleText"
                )
                .Default($"require {name}");
            condition.VisibilityRuleText = I18n.GetByKey(
                    $"BuiltinConditions.{name}.VisibilityRuleText"
                )
                .Default($"are visible with {name}");
        }
    }

    private static void OnLoadRuleSets(Dictionary<string, RuleSet> ruleSets)
    {
        foreach (var (name, ruleSet) in ruleSets)
        {
            ruleSet.Title = I18n.GetByKey($"BuiltinRuleSets.{name}.Title").Default(name);
            ruleSet.Description = I18n.GetByKey($"BuiltinRuleSets.{name}.Description")
                .UsePlaceholder(false)
                .Default(null);
        }
    }
}
