using StardewModdingAPI.Events;

namespace FishinC.Data;

class LazyAsset<T>
    where T : class
{
    public T Asset => GetOrLoadAsset();

    private readonly IGameContentHelper gameContent;
    private readonly IModContentHelper modContent;
    private readonly string name;
    private readonly Action<T>? onLoad;
    private readonly string physicalPath;

    private T? asset;

    public LazyAsset(IModHelper helper, string name, string physicalPath, Action<T>? onLoad = null)
    {
        gameContent = helper.GameContent;
        modContent = helper.ModContent;
        this.name = name;
        this.onLoad = onLoad;
        this.physicalPath = physicalPath;
        helper.Events.Content.AssetRequested += ContentEvents_AssetRequested;
        helper.Events.Content.AssetsInvalidated += ContentEvents_AssetsInvalidated;
    }

    private void ContentEvents_AssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(name))
        {
            e.LoadFrom(
                () =>
                {
                    var baseAsset = modContent.Load<T>(physicalPath);
                    onLoad?.Invoke(baseAsset);
                    return baseAsset;
                },
                AssetLoadPriority.Exclusive
            );
        }
    }

    private void ContentEvents_AssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(assetName => assetName.IsEquivalentTo(name)))
        {
            asset = null;
        }
    }

    private T GetOrLoadAsset()
    {
        return asset is not null ? asset : gameContent.Load<T>(name);
    }
}
