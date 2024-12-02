using FishinC.Configuration;
using HarmonyLib;

namespace FishinC.Integrations.Gmcm;

internal static class GmcmIntegration
{
    public static void Register(IManifest mod, IConfigurationContainer<ModConfig> configContainer)
    {
        if (Apis.Gmcm is not { } gmcm || Apis.ViewEngine is null)
        {
            return;
        }
        var harmony = new Harmony($"{mod.UniqueID}-GMCM");
        GmcmPatches.Apply(harmony, gmcm, mod);
        gmcm.Register(mod, () => { }, () => { });
        gmcm.AddParagraph(
            mod,
            () => I18n.Gmcm_Placeholder(mod.Name, configContainer.Config.SettingsKeybind)
        );
    }
}
