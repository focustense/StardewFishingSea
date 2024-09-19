using GenericModConfigMenu;

namespace FishingBuddy.Integrations;

internal static class Apis
{
    public static IGenericModConfigMenuApi? Gmcm { get; set; }

    // Must be called by ModEntry on game launch.
    public static void LoadAll(IModRegistry registry)
    {
        Gmcm = registry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
    }
}
