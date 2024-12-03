using FishinC.Configuration;
using FishinC.Data;
using FishinC.Integrations;
using FishinC.Integrations.Gmcm;
using FishinC.Integrations.StardewUI;
using FishinC.Patches;
using FishinC.Predictions;
using FishinC.UI;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Tools;

namespace FishinC;

internal sealed class ModEntry : Mod
{
    private CatchPreview CatchPreview => catchPreview.Value;
    private ModConfig Config => configContainer.Config;
    private RuleSet CurrentRules => configContainer.Config.Rules;
    private FishingState FishingState => fishingState.Value;
    private IViewDrawable? SeedFishPreviewDrawable => seedFishPreviewDrawable.Value;
    private SeedFishInfoViewModel SeedFishPreviewViewModel => seedFishPreviewViewModel.Value;
    private Splash? Splash
    {
        get => splash.Value;
        set => splash.Value = value;
    }
    private IViewDrawable? SplashInfoDrawable => splashInfoDrawable.Value;
    private SplashInfoViewModel SplashInfoViewModel => splashInfoViewModel.Value;

    private readonly PerScreen<Splash?> splash = new();
    private readonly Dictionary<string, IReadOnlyList<Splash>> splashSchedules = [];

    // Initialized in Entry
    private FishingApi api = null!;
    private PerScreen<CatchPreview> catchPreview = null!;
    private ConfigurationContainer<ModConfig> configContainer = null!;
    private ModData data = null!;
    private PerScreen<FishingState> fishingState = null!;
    private PerScreen<IViewDrawable?> seedFishPreviewDrawable = null!;
    private PerScreen<SeedFishInfoViewModel> seedFishPreviewViewModel = null!;
    private PerScreen<IViewDrawable?> splashInfoDrawable = null!;
    private PerScreen<SplashInfoViewModel> splashInfoViewModel = null!;
    private static TimeAccelerator timeAccelerator = null!;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        Quotes.Init(helper.Translation);
        data = new(helper);
        configContainer = new(helper);
        timeAccelerator = new(() => Config);
        var sideEffects = new SideEffectRegistry(Monitor);
        catchPreview = new(() => new(() => Config, sideEffects));
        seedFishPreviewViewModel = new(() => new());
        seedFishPreviewDrawable = new(
            () => CreateViewDrawable("SeedFishInfo", SeedFishPreviewViewModel)
        );
        splashInfoViewModel = new(() => new());
        splashInfoDrawable = new(() => CreateViewDrawable("SplashInfo", SplashInfoViewModel));
        fishingState = new(() =>
        {
            var state = new FishingState();
            state.Cast += FishingState_Cast;
            state.Cancelled += FishingState_Cancelled;
            state.Caught += FishingState_Caught;
            state.Lost += FishingState_Lost;
            return state;
        });
        api = new(sideEffects);

        helper.Events.Display.RenderedHud += Display_RenderedHud;
        helper.Events.Display.RenderedStep += Display_RenderedStep;
        helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
        helper.Events.Player.Warped += Player_Warped;

        PatchState.Monitor = Monitor;
        try
        {
            Patcher.ApplyAll(ModManifest.UniqueID);
        }
        catch (Exception ex)
        {
            Monitor.Log(
                $"Harmony patching failed; parts of the mod may function incorrectly.\n\n"
                    + $"The specific exception was: {ex}",
                LogLevel.Error
            );
        }

        GameStateQuery.Register(
            $"{ModManifest.UniqueID}_PLAYER_USING_TACKLE",
            Queries.PLAYER_USING_TACKLE
        );
    }

    public override object? GetApi()
    {
        return api;
    }

    private void Display_RenderedHud(object? sender, RenderedHudEventArgs e)
    {
        DrawSeedFishPreview(e.SpriteBatch);
    }

    private void Display_RenderedStep(object? sender, RenderedStepEventArgs e)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }
        if (e.Step == StardewValley.Mods.RenderSteps.World_Background)
        {
            DrawCatchPreviews(e.SpriteBatch);
        }
        else if (e.Step == StardewValley.Mods.RenderSteps.World_AlwaysFront)
        {
            DrawSplashPreview(e.SpriteBatch);
        }
    }

    private void FishingState_Cancelled(object? sender, EventArgs e)
    {
        CatchPreview.Frozen = false;
        // Always update in case respawns were frozen during the last cast and we are now overdue.
        // We just won't force the immediate update unless the rules say so.
        CatchPreview.Update(forceImmediateUpdate: CurrentRules.RespawnOnCancel);
    }

    private void FishingState_Cast(object? sender, EventArgs e)
    {
        if (CurrentRules.FreezeOnCast)
        {
            CatchPreview.Frozen = true;
        }
    }

    private void FishingState_Caught(object? sender, EventArgs e)
    {
        CatchPreview.Frozen = false;
        CatchPreview.Update(forceImmediateUpdate: true);
    }

    private void FishingState_Lost(object? sender, EventArgs e)
    {
        CatchPreview.Frozen = false;
        CatchPreview.Update(forceImmediateUpdate: true);
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        splashSchedules.Clear();
        RefreshForCurrentLocation();
    }

    private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        Apis.LoadAll(Helper.ModRegistry);
        GmcmIntegration.Register(ModManifest, configContainer);
        ViewEngineIntegration.Register(ModManifest, configContainer, () => data, Monitor);
    }

    private void GameLoop_ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        catchPreview.ResetAllScreens();
        splash.ResetAllScreens();
        seedFishPreviewDrawable.ResetAllScreens();
        seedFishPreviewViewModel.ResetAllScreens();
        splashInfoDrawable.ResetAllScreens();
        splashInfoViewModel.ResetAllScreens();
        FishRandom.ResetAllScreens();
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (Config.EnablePreviewsOnLoad)
        {
            CatchPreview.Enabled = true;
        }
    }

    private void GameLoop_TimeChanged(object? sender, TimeChangedEventArgs e)
    {
        UpdateNextSplash();
    }

    private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        var rod = Game1.player.CurrentTool as FishingRod;
        timeAccelerator.Active =
            IsEffectivelySinglePlayer()
            && Game1.CurrentEvent is null
            && FishingState.IsWaitingForBite();
        timeAccelerator.Update(rod);
        if (!Context.IsWorldReady)
        {
            return;
        }
        FishingState.Update(rod);
        CatchPreview.Update();
        UpdateSeededRandomFishPreview();
    }

    private void Input_ButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }
        if (Config.CatchPreviewToggleKeybind.JustPressed())
        {
            CatchPreview.Enabled = !CatchPreview.Enabled;
            Game1.addHUDMessage(
                new(CatchPreview.Enabled ? I18n.HudMessage_Enabled() : I18n.HudMessage_Disabled())
                {
                    noIcon = true,
                }
            );
            Helper.Input.SuppressActiveKeybinds(Config.CatchPreviewToggleKeybind);
        }
        else if (
            Context.IsPlayerFree
            && Game1.activeClickableMenu is null
            && Config.SettingsKeybind.JustPressed()
        )
        {
            ViewEngineIntegration.OpenSettingsMenu();
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

    private bool CheckRuleCondition(Func<RuleSet, string> conditionNameSelector)
    {
        var conditionName = conditionNameSelector(CurrentRules);
        return data.FeatureConditions.TryGetValue(conditionName, out var condition)
            && GameStateQuery.CheckConditions(condition.Query);
    }

    private IViewDrawable? CreateViewDrawable(string viewName, object context)
    {
        var drawable = Apis.ViewEngine?.CreateDrawableFromAsset(
            $"Mods/{ModManifest.UniqueID}/Views/{viewName}"
        );
        if (drawable is null)
        {
            return null;
        }
        drawable.Context = context;
        drawable.MaxSize = new(500, 500);
        return drawable;
    }

    private void DrawCatchPreviews(SpriteBatch spriteBatch)
    {
        if (
            !CatchPreview.Enabled
            || !Game1.currentLocation.canFishHere()
            || Game1.player.CurrentTool is not FishingRod
            || !CheckRuleCondition(rules => rules.FishPredictions)
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

    private void DrawSeedFishPreview(SpriteBatch spriteBatch)
    {
        if (
            !CatchPreview.Enabled
            || string.IsNullOrEmpty(SeedFishPreviewViewModel.FishId)
            || !Game1.currentLocation.canFishHere()
            || !CheckRuleCondition(rules => rules.JellyPredictions)
            || SeedFishPreviewDrawable is null
        )
        {
            return;
        }
        var position = GetViewportPosition(
            Config.SeededRandomFishHudPlacement,
            SeedFishPreviewDrawable.ActualSize
        );
        SeedFishPreviewDrawable.Draw(spriteBatch, position);
    }

    private void DrawSplashPreview(SpriteBatch spriteBatch)
    {
        if (
            !CatchPreview.Enabled
            || Splash is null
            || (
                Splash.StartTimeOfDay <= Game1.timeOfDay
                && !CheckRuleCondition(rules => rules.CurrentBubbles)
            )
            || (
                Splash.StartTimeOfDay > Game1.timeOfDay
                && !CheckRuleCondition(rules => rules.FutureBubbles)
            )
            || SplashInfoDrawable is null
        )
        {
            return;
        }
        var splashTilePos = new Vector2(Splash.Tile.X * 64, Splash.Tile.Y * 64);
        var splashTileCenter = splashTilePos + new Vector2(32, 32);
        var overlayPos = new Vector2(
            splashTileCenter.X - SplashInfoDrawable.ActualSize.X / 2,
            splashTileCenter.Y - SplashInfoDrawable.ActualSize.Y - 16
        );
        var translation = Game1.GlobalToLocal(Game1.viewport, overlayPos);
        SplashInfoDrawable.Draw(spriteBatch, translation);
    }

    private static Vector2 GetViewportPosition(NineGridPlacement placement, Vector2 contentSize)
    {
        var deviceViewport = Game1.graphics.GraphicsDevice.Viewport;
        var uiViewport = Game1.uiViewport;
        var viewportWidth = Math.Min(deviceViewport.Width, uiViewport.Width);
        var viewportHeight = Math.Min(deviceViewport.Height, uiViewport.Height);
        return placement.GetPosition(new(viewportWidth, viewportHeight), contentSize);
    }

    private static bool IsEffectivelySinglePlayer()
    {
        return !Context.IsMultiplayer || (!Context.IsSplitScreen && !Context.HasRemotePlayers);
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
            Splash = schedule.FirstOrDefault(s => s.EndTimeOfDay > Game1.timeOfDay);
        }
        UpdateSplashOverlay();
    }

    private void UpdateSeededRandomFishPreview()
    {
        if (CatchPreview.SeededRandomFish.Count > 0)
        {
            SeedFishPreviewViewModel.FishId = CatchPreview.SeededRandomFish[0].QualifiedItemId;
            SeedFishPreviewViewModel.CatchesRemaining = CatchPreview.SeededRandomCatchesRequired;
        }
        else
        {
            SeedFishPreviewViewModel.FishId = null;
            SeedFishPreviewViewModel.CatchesRemaining = 0;
        }
    }

    private void UpdateSplashOverlay()
    {
        if (Splash is not { } currentSplash)
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
        SplashInfoViewModel.FrenzyFishId = currentSplash.FrenzyFishId;
        SplashInfoViewModel.DurationMinutes = durationMinutes;
    }
}
