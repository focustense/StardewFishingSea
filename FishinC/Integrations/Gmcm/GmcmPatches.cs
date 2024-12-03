using FishinC.Integrations.StardewUI;
using GenericModConfigMenu;
using HarmonyLib;

namespace FishinC.Integrations.Gmcm;

internal static class GmcmPatches
{
    delegate void OpenListMenuDelegate(int? listScrollRow);

    // Initialized in Apply.
    private static IManifest modManifest = null!;

    // Lazily initialized.
    private static OpenListMenuDelegate? openListMenu;

    public static void Apply(Harmony harmony, IGenericModConfigMenuApi api, IManifest modManifest)
    {
        GmcmPatches.modManifest = modManifest;

        var assembly = new Traverse(api).Field("__Target").GetValue().GetType().Assembly;
        var modType = assembly.GetTypes().First(type => typeof(Mod).IsAssignableFrom(type));
        var openModMenuMethod = AccessTools.Method(modType, "OpenModMenu");
        harmony.Patch(
            openModMenuMethod,
            prefix: new(typeof(GmcmPatches), nameof(OpenModMenu_Prefix))
        );
        var openModMenuNewMethod = AccessTools.Method(modType, "OpenModMenuNew");
        if (openModMenuNewMethod is not null)
        {
            harmony.Patch(
                openModMenuNewMethod,
                prefix: new(typeof(GmcmPatches), nameof(OpenModMenu_Prefix))
            );
        }
    }

    private static bool OpenModMenu_Prefix(object __instance, IManifest mod, int? listScrollRow)
    {
        if (mod != modManifest)
        {
            return true;
        }
        if (openListMenu is null)
        {
            var openListMethod = AccessTools.Method(__instance.GetType(), "OpenListMenu");
            openListMenu = openListMethod.CreateDelegate<OpenListMenuDelegate>(__instance);
        }
        ViewEngineIntegration.OpenSettingsMenu(() => openListMenu(listScrollRow));
        return false;
    }
}
