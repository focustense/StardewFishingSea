using FishinC.UI;
using GenericModConfigMenu;
using StarControl;

namespace FishinC.Integrations;

internal static class Apis
{
    public static IGenericModConfigMenuApi? Gmcm { get; set; }
    public static IStarControlApi? StarControl { get; set; }
    public static IViewEngine? ViewEngine { get; set; }

    // Must be called by ModEntry on game launch.
    public static void LoadAll(IModRegistry registry)
    {
        Gmcm = registry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        StarControl = registry.GetApi<IStarControlApi>("focustense.StarControl");
        ViewEngine = registry.GetApi<IViewEngine>("focustense.StardewUI");
    }
}
