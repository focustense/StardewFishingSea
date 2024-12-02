using FishinC.Configuration;
using FishinC.Data;
using FishinC.UI;
using StardewValley.Menus;

namespace FishinC.Integrations.StardewUI;

internal static class ViewEngineIntegration
{
    // Assigned in Register; always assumed to be non-null before any other calls.
    private static IConfigurationContainer<ModConfig> configContainer = null!;
    private static Func<ModData> dataSelector = null!;
    private static IManifest mod = null!;
    private static IMonitor monitor = null!;

    public static void Register(
        IManifest mod,
        IConfigurationContainer<ModConfig> configContainer,
        Func<ModData> dataSelector,
        IMonitor monitor
    )
    {
        ViewEngineIntegration.configContainer = configContainer;
        ViewEngineIntegration.dataSelector = dataSelector;
        ViewEngineIntegration.mod = mod;
        ViewEngineIntegration.monitor = monitor;
        if (Apis.ViewEngine is not { } viewEngine)
        {
            monitor.Log(
                "StardewUI is not installed or failed to load; most UI functions will be disabled.",
                LogLevel.Error
            );
            return;
        }
        viewEngine.RegisterSprites($"Mods/{mod.UniqueID}/Sprites", "assets/sprites");
        viewEngine.RegisterViews($"Mods/{mod.UniqueID}/Views", "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    public static void OpenSettingsMenu(Action? closeFromTitle = null)
    {
        if (Apis.ViewEngine is not { } viewEngine)
        {
            monitor.Log(
                "Can't open settings menu because StardewUI is not installed or failed to load.",
                LogLevel.Error
            );
            return;
        }
        var close = () => CloseSettingsMenu(closeFromTitle);
        var viewModel = new SettingsViewModel(configContainer, dataSelector(), close);
        var controller = viewEngine.CreateMenuControllerFromAsset(
            $"Mods/{mod.UniqueID}/Views/Settings",
            viewModel
        );
        controller.CloseAction = close;
        controller.SetGutters(100, 64);
        if (Game1.activeClickableMenu is TitleMenu)
        {
            TitleMenu.subMenu = controller.Menu;
            return;
        }
        var parentMenu = GetFrontMenu();
        if (parentMenu is not null)
        {
            parentMenu.SetChildMenu(controller.Menu);
        }
        else
        {
            Game1.activeClickableMenu = controller.Menu;
        }
    }

    private static void CloseSettingsMenu(Action? closeFromTitle)
    {
        if (Game1.activeClickableMenu is TitleMenu)
        {
            if (closeFromTitle is not null)
            {
                closeFromTitle();
            }
            else
            {
                TitleMenu.subMenu = null;
            }
            return;
        }
        var settingsMenu = GetFrontMenu();
        var parentMenu = settingsMenu?.GetParentMenu();
        if (parentMenu is not null)
        {
            parentMenu.SetChildMenu(null);
        }
        else
        {
            Game1.exitActiveMenu();
        }
    }

    private static IClickableMenu? GetFrontMenu()
    {
        IClickableMenu? menu = null;
        for (
            menu = Game1.activeClickableMenu;
            menu?.GetChildMenu() is not null;
            menu = menu.GetChildMenu()
        ) { }
        return menu;
    }
}
