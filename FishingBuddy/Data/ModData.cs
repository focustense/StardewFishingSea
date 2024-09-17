using Microsoft.Xna.Framework.Graphics;
using StardewUI;

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
    /// Sprite sheet containing the sprites used for mouse buttons.
    /// </summary>
    public Texture2D MouseButtons => mouseButtonsTexture.Asset;

    /// <summary>
    /// Sprite sheet containing arrows used in button prompts.
    /// </summary>
    public Texture2D PromptArrows => promptArrowsTexture.Asset;

    /// <summary>
    /// Dictionary of built-in rule set names to their rules.
    /// </summary>
    public Dictionary<string, RuleSet> RuleSets => ruleSets.Asset;

    private readonly LazyAsset<Dictionary<string, FeatureCondition>> featureConditions =
        new(
            helper,
            $"Mods/{helper.ModContent.ModID}/Conditions",
            "assets/conditions.json",
            conditions => OnLoadFeatureConditions(conditions, helper.ModRegistry.ModID)
        );

    private readonly LazyAsset<Texture2D> keybindButtonsTexture =
        new(helper, $"Mods/{helper.ModContent.ModID}/KeybindButtons", "assets/XboxButtons.png");

    private readonly LazyAsset<Texture2D> keybindKeysTexture =
        new(helper, $"Mods/{helper.ModContent.ModID}/KeybindKeys", "assets/KeyboardKeys.png");

    private readonly LazyAsset<Texture2D> mouseButtonsTexture =
        new(helper, $"Mods/{helper.ModContent.ModID}/MouseButtons", "assets/MouseButtons.png");

    private readonly LazyAsset<Texture2D> promptArrowsTexture =
        new(helper, $"Mods/{helper.ModContent.ModID}/PromptArrows", "assets/PromptArrows.png");

    private readonly LazyAsset<Dictionary<string, RuleSet>> ruleSets =
        new(helper, $"Mods/{helper.ModContent.ModID}/Rules", "assets/rules.json", OnLoadRuleSets);

    /// <summary>
    /// Gets the sprite map for gamepad buttons/keyboard keys used in keybinds/button prompts.
    /// </summary>
    /// <param name="overlay">Whether to configure the map for overlays, vs. the default used
    /// in menus. Overlay themes use dark keyboard and light mouse sprites, and use a larger
    /// slice scale for keyboard keys in order to remain uniform.</param>
    public ISpriteMap<SButton> GetButtonSpriteMap(bool overlay = false)
    {
        return new XeluButtonSpriteMap(KeybindButtons, KeybindKeys, MouseButtons)
        {
            KeyboardTheme = overlay
                ? XeluButtonSpriteMap.SpriteTheme.Dark
                : XeluButtonSpriteMap.SpriteTheme.Stardew,
            MouseTheme = overlay
                ? XeluButtonSpriteMap.SpriteTheme.Light
                : XeluButtonSpriteMap.SpriteTheme.Stardew,
            SliceScale = overlay ? 1.0f : 0.3f,
        };
    }

    /// <summary>
    /// Gets the sprite map for arrows used in positioning and other UI prompts.
    /// </summary>
    /// <returns></returns>
    public ISpriteMap<Direction> GetDirectionSpriteMap()
    {
        return new SpriteMapBuilder<Direction>(PromptArrows)
            .Size(64, 64)
            .Add(Direction.North, Direction.South, Direction.West, Direction.East)
            .Build();
    }

    private static void OnLoadFeatureConditions(
        Dictionary<string, FeatureCondition> featureConditions,
        string modId
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
            condition.Query = condition.Query.Replace("{{ModId}}", modId);
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
