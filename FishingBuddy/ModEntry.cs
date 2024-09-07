using FishingBuddy.UI;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewUI;
using StardewValley.Internal;

namespace FishingBuddy;

internal sealed class ModEntry : Mod
{
    private static readonly Vector2 SPLASH_OVERLAY_MAX_SIZE = new(500, 500);

    private readonly Dictionary<string, IReadOnlyList<Splash>> splashSchedules = [];

    // Initialized in Entry
    private ModConfig config = null!;

    private PerScreen<Splash?> splash = new();
    private PerScreen<SpeechBubble<SplashInfoView>> splashOverlay = new(CreateSplashOverlay);

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        Logger.Monitor = Monitor;

        helper.Events.Display.RenderedWorld += Display_RenderedWorld;
        helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
        helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        helper.Events.Player.Warped += Player_Warped;

        var harmony = new Harmony(ModManifest.UniqueID);
        // GetFishFromLocation data is public, but the overload containing the real
        // implementation is internal.
        var getFishFromLocationDataMethod = AccessTools.Method(
            typeof(GameLocation),
            nameof(GameLocation.GetFishFromLocationData),
            [
                typeof(string), // locationName
                typeof(Vector2), // bobberTile
                typeof(int), // waterDepth
                typeof(Farmer), // player
                typeof(bool), // isTutorialCatch
                typeof(bool), // isInherited
                typeof(GameLocation),
                typeof(ItemQueryContext),
            ]
        );
        var allGameRandomRefsTranspilerMethod = new HarmonyMethod(
            typeof(LocationFishPatches),
            nameof(LocationFishPatches.AllGameRandomRefsTranspiler)
        );
        harmony.Patch(getFishFromLocationDataMethod, transpiler: allGameRandomRefsTranspilerMethod);
        var orderings = LocationFishPatches.GetOrderByMethods(getFishFromLocationDataMethod);
        foreach (var orderMethod in orderings)
        {
            harmony.Patch(orderMethod, transpiler: allGameRandomRefsTranspilerMethod);
        }
    }

    private void Display_RenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        var splash = this.splash.Value;
        if (splash is null)
        {
            return;
        }
        var splashOverlay = this.splashOverlay.Value;
        splashOverlay.Measure(SPLASH_OVERLAY_MAX_SIZE);
        var splashTilePos = new Vector2(splash.Tile.X * 64, splash.Tile.Y * 64);
        var splashTileCenter = splashTilePos + new Vector2(32, 32);
        var overlayPos = new Vector2(
            splashTileCenter.X - splashOverlay.OuterSize.X / 2,
            splashTileCenter.Y - splashOverlay.OuterSize.Y - 16
        );
        var translation = Game1.GlobalToLocal(Game1.viewport, overlayPos);
        var overlayBatch = new PropagatedSpriteBatch(
            e.SpriteBatch,
            Transform.FromTranslation(translation)
        );
        splashOverlay.Draw(overlayBatch);
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        splashSchedules.Clear();
        RefreshForCurrentLocation();
    }

    private void GameLoop_ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        splash.ResetAllScreens();
    }

    private void GameLoop_TimeChanged(object? sender, TimeChangedEventArgs e)
    {
        UpdateNextSplash();
    }

    private void Player_Warped(object? sender, WarpedEventArgs e)
    {
        RefreshForCurrentLocation();
    }

    private void AddSplashData(GameLocation location)
    {
        if (splashSchedules.ContainsKey(location.NameOrUniqueName))
        {
            return;
        }
        var splashes = SplashPredictor.PredictSplashes(location).ToList();
        splashSchedules.Add(location.NameOrUniqueName, splashes);
    }

    private static SpeechBubble<SplashInfoView> CreateSplashOverlay()
    {
        return new() { Content = new SplashInfoView() };
    }

    private void RefreshForCurrentLocation()
    {
        splash.Value = null;
        AddSplashData(Game1.currentLocation);
        UpdateNextSplash();
    }

    private void UpdateNextSplash()
    {
        if (splash.Value is not null)
        {
            if (splash.Value.EndTimeOfDay <= Game1.timeOfDay)
            {
                splash.Value = null;
            }
            else
            {
                UpdateSplashOverlay();
                return;
            }
        }
        if (splashSchedules.TryGetValue(Game1.currentLocation.NameOrUniqueName, out var schedule))
        {
            splash.Value = schedule.FirstOrDefault(s => s.StartTimeOfDay >= Game1.timeOfDay);
        }
        UpdateSplashOverlay();
    }

    private void UpdateSplashOverlay()
    {
        if (splash.Value is not Splash currentSplash)
        {
            return;
        }
        var durationMinutes =
            currentSplash.StartTimeOfDay > Game1.timeOfDay
                ? -Utility.CalculateMinutesBetweenTimes(
                    Game1.timeOfDay,
                    currentSplash.StartTimeOfDay
                )
                : Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, currentSplash.EndTimeOfDay);
        splashOverlay.Value.Content!.FrenzyFishId = currentSplash.FrenzyFishId;
        splashOverlay.Value.Content!.DurationMinutes = durationMinutes;
    }
}
