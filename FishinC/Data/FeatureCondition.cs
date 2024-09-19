namespace FishinC.Data;

/// <summary>
/// Game/progression condition to unlock a feature.
/// </summary>
public class FeatureCondition
{
    /// <summary>
    /// Localized, simple, short, self-contained name for the condition, displayed in drop-downs.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Localized text to display in rule text written as a visibility variant, e.g. "{{Feature}}
    /// requires {{RequirementRuleText}}."
    /// </summary>
    public string RequirementRuleText { get; set; } = "";

    /// <summary>
    /// Localized text to display in rule text written as a requirement variant, e.g. "{{Feature}}
    /// is visible with {{VisibilityRuleText}}."
    /// </summary>
    public string VisibilityRuleText { get; set; } = "";

    /// <summary>
    /// The query (GSQ) implementing the actual condition.
    /// </summary>
    public string Query { get; set; } = "";
}
