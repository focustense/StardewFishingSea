using FishinC.Configuration;
using FishinC.Data;
using GenericModConfigMenu;
using HarmonyLib;

namespace FishinC.Integrations.Gmcm;

internal static class GmcmIntegration
{
    public static void Register(
        IManifest mod,
        Func<ModData> dataSelector,
        IConfigurationContainer<ModConfig> configContainer
    )
    {
        if (Apis.Gmcm is not IGenericModConfigMenuApi gmcm)
        {
            return;
        }
        var harmony = new Harmony($"{mod.UniqueID}-GMCM");
        GmcmPatches.Apply(harmony, gmcm, mod, dataSelector, configContainer);
        gmcm.Register(mod, () => { }, () => { });
        gmcm.AddParagraph(mod, () => I18n.Gmcm_Placeholder(configContainer.Config.SettingsKeybind));
    }
}
