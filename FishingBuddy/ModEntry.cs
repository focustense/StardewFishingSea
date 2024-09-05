using StardewModdingAPI.Events;

namespace FishingBuddy;

internal sealed class ModEntry : Mod
{
    private readonly Dictionary<string, IReadOnlyList<Splash>> splashSchedules = [];

    // Initialized in Entry
    private ModConfig config = null!;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        helper.Events.Player.Warped += Player_Warped;
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        splashSchedules.Clear();
        AddSplashData(Game1.currentLocation);
    }

    private void Player_Warped(object? sender, WarpedEventArgs e)
    {
        AddSplashData(e.NewLocation);
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
}
