using FishinC.Predictions;

namespace FishinC;

/// <summary>
/// Public API for FishinC.
/// </summary>
public interface IFishingApi
{
    /// <summary>
    /// Sub-API for side effects. See <see cref="IFishSideEffect"/>.
    /// </summary>
    ISideEffectsApi SideEffects { get; }
}

/// <summary>
/// Implementation of the public API.
/// </summary>
public class FishingApi(ISideEffectsApi sideEffects) : IFishingApi
{
    public ISideEffectsApi SideEffects { get; } = sideEffects;
}
