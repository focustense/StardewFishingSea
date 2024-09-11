using StardewUI;

namespace FishingBuddy.UI;

/// <summary>
/// Menu for the mod configuration.
/// </summary>
/// <param name="configContainer">Configuration container for the mod.</param>
internal class SettingsMenu(IConfigurationContainer<ModConfig> configContainer)
    : ViewMenu<SettingsView>
{
    protected override SettingsView CreateView()
    {
        return new(configContainer);
    }
}
