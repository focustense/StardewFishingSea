using System.ComponentModel;
using FishinC.Data;
using PropertyChanged.SourceGenerator;

namespace FishinC.UI;

/// <summary>
/// View model for the custom rules editor.
/// </summary>
/// <param name="data">The current mod data.</param>
/// <param name="initialRules">Initial rules to display in the UI; the saved rules.</param>
public partial class CustomRuleSetViewModel(ModData data, RuleSet initialRules)
    : INotifyPropertyChanged
{
    /// <summary>
    /// Function that takes an element of <see cref="ConditionKeys"/> and formats it for display in
    /// the drop-down.
    /// </summary>
    public Func<string, string> ConditionFormat { get; } = key => data.FeatureConditions[key].Title;

    /// <summary>
    /// List of keys available to use as conditions for rules such as
    /// <see cref="FishPredictionsCondition"/>.
    /// </summary>
    public IReadOnlyList<string> ConditionKeys { get; } = [.. data.FeatureConditions.Keys];

    /// <inheritdoc cref="RuleSet.CurrentBubbles"/>
    [Notify]
    private string currentBubblesCondition = initialRules.CurrentBubbles;

    /// <inheritdoc cref="RuleSet.FishPredictions"/>
    [Notify]
    private string fishPredictionsCondition = initialRules.FishPredictions;

    /// <inheritdoc cref="RuleSet.FreezeOnCast"/>
    [Notify]
    private bool freezeOnCast = initialRules.FreezeOnCast;

    /// <inheritdoc cref="RuleSet.FutureBubbles"/>
    [Notify]
    private string futureBubblesCondition = initialRules.FutureBubbles;

    /// <inheritdoc cref="RuleSet.JellyPredictions"/>
    [Notify]
    private string jellyPredictionsCondition = initialRules.JellyPredictions;

    /// <inheritdoc cref="RuleSet.RespawnOnCancel"/>
    [Notify]
    private bool respawnOnCancel = initialRules.RespawnOnCancel;

    /// <summary>
    /// Resets all rules in the UI to their saved values.
    /// </summary>
    /// <param name="rules">The default rules.</param>
    public void Reset(RuleSet rules)
    {
        CurrentBubblesCondition = rules.CurrentBubbles;
        FutureBubblesCondition = rules.FutureBubbles;
        FishPredictionsCondition = rules.FishPredictions;
        JellyPredictionsCondition = rules.JellyPredictions;
        FreezeOnCast = rules.FreezeOnCast;
        RespawnOnCancel = rules.RespawnOnCancel;
    }

    /// <summary>
    /// Creates a new <see cref="RuleSet"/> from the current UI selections.
    /// </summary>
    public RuleSet ToConfig()
    {
        return new()
        {
            CurrentBubbles = CurrentBubblesCondition,
            FutureBubbles = FutureBubblesCondition,
            FishPredictions = FishPredictionsCondition,
            JellyPredictions = JellyPredictionsCondition,
            FreezeOnCast = FreezeOnCast,
            RespawnOnCancel = RespawnOnCancel,
        };
    }
}
