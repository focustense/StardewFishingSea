using FishingBuddy.UI;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewUI;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Tools;

namespace FishingBuddy;

internal sealed class ModEntry : Mod
{
    private CatchPreview CatchPreview => catchPreview.Value;
    private Splash? Splash
    {
        get => splash.Value;
        set => splash.Value = value;
    }
    private SpeechBubble<SplashInfoView> SplashOverlay => splashOverlay.Value;

    private static readonly Vector2 SplashOverlayMaxSize = new(500, 500);

    private readonly Dictionary<string, IReadOnlyList<Splash>> splashSchedules = [];

    // Initialized in Entry
    private ModConfig config = null!;

    private readonly PerScreen<CatchPreview> catchPreview;
    private readonly PerScreen<Splash?> splash = new();
    private readonly PerScreen<SpeechBubble<SplashInfoView>> splashOverlay =
        new(CreateSplashOverlay);

    // Constructor is only to initialize certain lazy-loaders.
    public ModEntry()
    {
        catchPreview = new(() => new CatchPreview(() => config));
    }

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        Logger.Monitor = Monitor;

        helper.Events.Display.RenderedStep += Display_RenderedStep;
        helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
        helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
        helper.Events.Player.Warped += Player_Warped;

        var harmony = new Harmony(ModManifest.UniqueID);
        // ItemQueryResolver.TryResolve has an apparent bug in one of its overloads that goes
        // directly to Game1.random instead of using context.Random which would point to the
        // replayable instance. Patch it to use the context.
        var itemQueryTryResolveMethod = AccessTools.Method(
            typeof(ItemQueryResolver),
            nameof(ItemQueryResolver.TryResolve),
            [
                typeof(string), // query
                typeof(ItemQueryContext),
                typeof(ItemQuerySearchMode),
                typeof(string), // perItemCondition
                typeof(int?), // maxItems
                typeof(bool), // avoidRepeat
                typeof(HashSet<string>), // avoidItemIds
                typeof(Action<string, string>), // logError
            ]
        );
        harmony.Patch(
            itemQueryTryResolveMethod,
            transpiler: new(typeof(ItemQueryPatches), nameof(ItemQueryPatches.TryResolveTranspiler))
        );
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
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), "CheckGenericFishRequirements"),
            transpiler: allGameRandomRefsTranspilerMethod
        );
        var orderings = LocationFishPatches.GetOrderByMethods(getFishFromLocationDataMethod);
        foreach (var orderMethod in orderings)
        {
            harmony.Patch(orderMethod, transpiler: allGameRandomRefsTranspilerMethod);
        }
        harmony.Patch(
            AccessTools.Method(
                typeof(MineShaft),
                nameof(MineShaft.getFish),
                [
                    typeof(float), // millisecondsAfterNibble
                    typeof(string), // bait
                    typeof(int), // waterDepth
                    typeof(Farmer),
                    typeof(double), // baitPotency
                    typeof(Vector2), // bobberTile
                    typeof(string), // locationName
                ]
            ),
            transpiler: allGameRandomRefsTranspilerMethod
        );
    }

    private void Display_RenderedStep(object? sender, RenderedStepEventArgs e)
    {
        if (e.Step == StardewValley.Mods.RenderSteps.World_Background)
        {
            DrawCatchPreviews(e.SpriteBatch);
            DrawSplashPreview(e.SpriteBatch);
        }
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        splashSchedules.Clear();
        RefreshForCurrentLocation();
    }

    private void GameLoop_ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        splash.ResetAllScreens();
        catchPreview.ResetAllScreens();
        FishRandom.ResetAllScreens();
    }

    private void GameLoop_TimeChanged(object? sender, TimeChangedEventArgs e)
    {
        UpdateNextSplash();
    }

    private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        CatchPreview.Update();
    }

    private void Input_ButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (config.CatchPreviewToggleKeybind.JustPressed())
        {
            CatchPreview.Enabled = !CatchPreview.Enabled;
            Monitor.Log(
                "Fish catch previews " + (CatchPreview.Enabled ? "enabled" : "disabled"),
                LogLevel.Debug
            );
            Helper.Input.SuppressActiveKeybinds(config.CatchPreviewToggleKeybind);
        }
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

    private void DrawCatchPreviews(SpriteBatch spriteBatch)
    {
        if (
            !CatchPreview.Enabled
            || !Game1.currentLocation.canFishHere()
            || (
                config.CatchPreviewVisibility == CatchPreviewVisibility.OnlyWhenRodSelected
                && Game1.player.CurrentTool is not FishingRod
            )
        )
        {
            return;
        }
        foreach (var prediction in CatchPreview.Predictions)
        {
            var fishData = ItemRegistry.GetDataOrErrorItem(prediction.FishId);
            var tilePos = Game1
                .GlobalToLocal(Game1.viewport, prediction.Tile.ToVector2() * Game1.tileSize)
                .ToPoint();
            var destinationRect = new Rectangle(tilePos, new(Game1.tileSize, Game1.tileSize));
            spriteBatch.Draw(
                fishData.GetTexture(),
                destinationRect,
                fishData.GetSourceRect(),
                new(0.5f, 0.5f, 0.5f, 0.5f)
            );
        }
    }

    private void DrawSplashPreview(SpriteBatch spriteBatch)
    {
        if (
            Splash is null
            || config.SplashPreviewVisibility == SplashPreviewVisibility.None
            || (
                config.SplashPreviewVisibility != SplashPreviewVisibility.RemainingAndUpcoming
                && Splash.StartTimeOfDay > Game1.timeOfDay
            )
        )
        {
            return;
        }
        SplashOverlay.Measure(SplashOverlayMaxSize);
        var splashTilePos = new Vector2(Splash.Tile.X * 64, Splash.Tile.Y * 64);
        var splashTileCenter = splashTilePos + new Vector2(32, 32);
        var overlayPos = new Vector2(
            splashTileCenter.X - SplashOverlay.OuterSize.X / 2,
            splashTileCenter.Y - SplashOverlay.OuterSize.Y - 16
        );
        var translation = Game1.GlobalToLocal(Game1.viewport, overlayPos);
        var overlayBatch = new PropagatedSpriteBatch(
            spriteBatch,
            Transform.FromTranslation(translation)
        );
        SplashOverlay.Draw(overlayBatch);
    }

    private void RefreshForCurrentLocation()
    {
        Splash = null;
        AddSplashData(Game1.currentLocation);
        UpdateNextSplash();
    }

    private void UpdateNextSplash()
    {
        if (Splash is not null)
        {
            if (Splash.EndTimeOfDay <= Game1.timeOfDay)
            {
                Splash = null;
            }
            else
            {
                UpdateSplashOverlay();
                return;
            }
        }
        if (splashSchedules.TryGetValue(Game1.currentLocation.NameOrUniqueName, out var schedule))
        {
            Splash = schedule.FirstOrDefault(s => s.StartTimeOfDay >= Game1.timeOfDay);
        }
        UpdateSplashOverlay();
    }

    private void UpdateSplashOverlay()
    {
        if (Splash is not Splash currentSplash)
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
        SplashOverlay.Content!.FrenzyFishId = currentSplash.FrenzyFishId;
        SplashOverlay.Content!.DurationMinutes = durationMinutes;
    }
}
