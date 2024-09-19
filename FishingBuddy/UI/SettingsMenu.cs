using FishingBuddy.Configuration;
using FishingBuddy.Data;
using Microsoft.Xna.Framework.Input;
using StardewUI;

namespace FishingBuddy.UI;

/// <summary>
/// Menu for the mod configuration.
/// </summary>
/// <param name="configContainer">Configuration container for the mod.</param>
internal class SettingsMenu(
    ModData data,
    IConfigurationContainer<ModConfig> configContainer,
    bool allowDefaultClose = true,
    Action? close = null
) : ViewMenu<SettingsView>(gutter: new(Top: 16, Bottom: 80))
{
    public override bool readyToClose()
    {
        return allowDefaultClose;
    }

    public override void receiveGamePadButton(Buttons b)
    {
        if (!allowDefaultClose && b == Buttons.B)
        {
            Close();
            return;
        }
        base.receiveGamePadButton(b);
    }

    public override void receiveKeyPress(Keys key)
    {
        if (!allowDefaultClose && Game1.options.menuButton.Any(b => b.key == key))
        {
            Close();
            return;
        }
        base.receiveKeyPress(key);
    }

    protected override SettingsView CreateView()
    {
        return new(data, configContainer, Close);
    }

    private new void Close()
    {
        if (close is not null)
        {
            close();
        }
        else
        {
            SetActive(false);
        }
    }
}
