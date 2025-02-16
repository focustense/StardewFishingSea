using Microsoft.Xna.Framework.Graphics;
using StarControl;
using StardewValley.ItemTypeDefinitions;

namespace FishinC.Integrations.StarControl;

internal static class StarControlIntegration
{
    public static void Register(IManifest mod, Action toggleOverlays)
    {
        if (Apis.StarControl is not { } starControl)
        {
            return;
        }
        starControl.RegisterItems(
            mod,
            [
                new RadialMenuItem(
                    $"{mod.UniqueID}.ToggleOverlays",
                    toggleOverlays,
                    ItemRegistry.GetDataOrErrorItem("(O)147")
                ),
            ]
        );
    }

    private class RadialMenuItem(string id, Action activate, ParsedItemData icon) : IRadialMenuItem
    {
        public string Id => id;

        public string Title => I18n.Settings_ModTitle();

        public string Description => I18n.StarControl_ToggleOverlays_Description();

        public Texture2D? Texture => icon.GetTexture();

        public Rectangle? SourceRectangle => icon.GetSourceRect();

        public ItemActivationResult Activate(
            Farmer who,
            DelayedActions delayedActions,
            ItemActivationType activationType = ItemActivationType.Primary
        )
        {
            if (delayedActions != DelayedActions.None)
            {
                return ItemActivationResult.Delayed;
            }
            activate();
            return ItemActivationResult.Custom;
        }
    }
}
