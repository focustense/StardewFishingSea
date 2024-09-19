using FishinC.Data;
using StardewUI;

namespace FishinC.UI;

internal class RulesForm(ModData data, RuleSet rules, int labelWidth) : WrapperView
{
    protected override IView CreateView()
    {
        var freezeOnCastCheckbox = new CheckBox() { IsChecked = rules.FreezeOnCast };
        freezeOnCastCheckbox.Change += FreezeOnCastCheckbox_Change;
        var respawnOnCancelCheckbox = new CheckBox() { IsChecked = rules.RespawnOnCancel };
        respawnOnCancelCheckbox.Change += RespawnOnCancelCheckbox_Change;
        return new FormBuilder(labelWidth, 0)
            .AddField(
                I18n.Settings_Rules_CurrentBubbles_Title(),
                I18n.Settings_Rules_CurrentBubbles_Description(),
                CreateConditionDropdown(r => r.CurrentBubbles, (r, v) => r.CurrentBubbles = v)
            )
            .AddField(
                I18n.Settings_Rules_FutureBubbles_Title(),
                I18n.Settings_Rules_FutureBubbles_Description(),
                CreateConditionDropdown(r => r.FutureBubbles, (r, v) => r.FutureBubbles = v)
            )
            .AddField(
                I18n.Settings_Rules_FishPredictions_Title(),
                I18n.Settings_Rules_FishPredictions_Description(),
                CreateConditionDropdown(r => r.FishPredictions, (r, v) => r.FishPredictions = v)
            )
            .AddField(
                I18n.Settings_Rules_JellyPredictions_Title(),
                I18n.Settings_Rules_JellyPredictions_Description(),
                CreateConditionDropdown(r => r.JellyPredictions, (r, v) => r.JellyPredictions = v)
            )
            .AddField(
                I18n.Settings_Rules_FreezeOnCast_Title(),
                I18n.Settings_Rules_FreezeOnCast_Description(),
                freezeOnCastCheckbox
            )
            .AddField(
                I18n.Settings_Rules_RespawnOnCancel_Title(),
                I18n.Settings_Rules_RespawnOnCancel_Description(),
                respawnOnCancelCheckbox
            )
            .Build();
    }

    private void ConditionDropdown_Select(object? sender, EventArgs e)
    {
        if (sender is not DropDownList<string> dropDownList || dropDownList.SelectedOption is null)
        {
            return;
        }
        var setCondition = dropDownList.Tags.Get<Action<RuleSet, string>>();
        setCondition?.Invoke(rules, dropDownList.SelectedOption);
    }

    private DropDownList<string> CreateConditionDropdown(
        Func<RuleSet, string> getCondition,
        Action<RuleSet, string> setCondition
    )
    {
        var dropDownList = new DropDownList<string>()
        {
            Options = [.. data.FeatureConditions.Keys],
            OptionFormat = name => data.FeatureConditions[name].Title,
            OptionMinWidth = 250,
            SelectedOption = getCondition(rules),
            Tags = Tags.Create(setCondition),
        };
        dropDownList.Select += ConditionDropdown_Select;
        return dropDownList;
    }

    private void FreezeOnCastCheckbox_Change(object? sender, EventArgs e)
    {
        rules.FreezeOnCast = (sender as CheckBox)?.IsChecked ?? false;
    }

    private void RespawnOnCancelCheckbox_Change(object? sender, EventArgs e)
    {
        rules.RespawnOnCancel = (sender as CheckBox)?.IsChecked ?? false;
    }
}
