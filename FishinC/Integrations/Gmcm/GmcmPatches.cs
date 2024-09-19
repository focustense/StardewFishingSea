using FishinC.Configuration;
using FishinC.Data;
using FishinC.UI;
using GenericModConfigMenu;
using HarmonyLib;

namespace FishinC.Integrations.Gmcm;

internal static class GmcmPatches
{
    delegate void OpenListMenuDelegate(int? listScrollRow);

    // Initialized in Init.
    private static IConfigurationContainer<ModConfig> configContainer = null!;
    private static Func<ModData> dataSelector = null!;
    private static IManifest modManifest = null!;

    // Initialized after first prefix.
    private static OpenListMenuDelegate openListMenu = null!;

    public static void Apply(
        Harmony harmony,
        IGenericModConfigMenuApi api,
        IManifest modManifest,
        Func<ModData> dataSelector,
        IConfigurationContainer<ModConfig> configContainer
    )
    {
        GmcmPatches.configContainer = configContainer;
        GmcmPatches.dataSelector = dataSelector;
        GmcmPatches.modManifest = modManifest;

        var assembly = new Traverse(api).Field("__Target").GetValue().GetType().Assembly;
        var modType = assembly.GetTypes().First(type => typeof(Mod).IsAssignableFrom(type));
        var openModMenuMethod = AccessTools.Method(modType, "OpenModMenu");
        harmony.Patch(
            openModMenuMethod,
            prefix: new(typeof(GmcmPatches), nameof(OpenModMenu_Prefix))
        );
    }

    private static bool OpenModMenu_Prefix(object __instance, IManifest mod, int? listScrollRow)
    {
        if (openListMenu is null)
        {
            var openListMethod = AccessTools.Method(__instance.GetType(), "OpenListMenu");
            openListMenu = openListMethod.CreateDelegate<OpenListMenuDelegate>(__instance);
        }
        if (mod == modManifest)
        {
            OpenSettingsMenu(() =>
            {
                Game1.playSound("bigDeSelect");
                openListMenu(listScrollRow);
            });
            return false;
        }
        return true;
    }

    private static void OpenSettingsMenu(Action onClose)
    {
        var menu = new SettingsMenu(
            dataSelector(),
            configContainer,
            allowDefaultClose: false,
            close: onClose
        );
        menu.SetActive(true);
    }
}
