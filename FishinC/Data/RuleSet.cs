using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace FishinC.Data;

/// <summary>
/// Configures the rules for access to various features.
/// </summary>
/// <remarks>
/// These rules generally only apply to difficulty/progression scaling. Other settings, like key
/// bindings, respwawn interval, HUD position, etc. are more about personal preference than "rules"
/// and are therefore top-level settings in the <see cref="Configuration.ModConfig"/>.
/// </remarks>
public class RuleSet
{
    /// <summary>
    /// Localized title for the rule set, shown as button text.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Localized description for the rule set, shown in tooltips.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Qualified ID of the item (generally, a fish) representing this rule set in UI/menus.
    /// </summary>>
    public string SpriteItemId { get; set; } = "";

    /// <summary>
    /// Name of the <see cref="FeatureCondition"/> for whether to show current splash spot
    /// durations.
    /// </summary>
    public string CurrentBubbles { get; set; } = "Always";

    /// <summary>
    /// Name of the <see cref="FeatureCondition"/> for whether to show an indicator where the next
    /// splash spot will appear, and how long before it starts.
    /// </summary>
    public string FutureBubbles { get; set; } = "Always";

    /// <summary>
    /// Name of the <see cref="FeatureCondition"/> for whether to predict (and RNG-lock) the next
    /// fish to be caught per tile.
    /// </summary>
    public string FishPredictions { get; set; } = "Always";

    /// <summary>
    /// Name of the <see cref="FeatureCondition"/> for whether to show availability of jellies
    /// (seeded-random fish) or the number of other catches required to obtain another.
    /// </summary>
    public string JellyPredictions { get; set; } = "Always";

    /// <summary>
    /// Whether fish catches/predictions should stay RNG-locked while the line is cast. A
    /// <c>false</c> value means they can change while waiting for a bite.
    /// </summary>
    public bool FreezeOnCast { get; set; } = true;

    /// <summary>
    /// Whether to force-respawn the random fish in an area after cancelling a cast, i.e. pulling
    /// in the line without a bite.
    /// </summary>
    /// <remarks>
    /// With this enabled, repeatedly casting and cancelling essentially becomes a way of trading
    /// energy for time.
    /// </remarks>
    public bool RespawnOnCancel { get; set; } = true;

    /// <summary>
    /// Creates a copy of this rule set.
    /// </summary>
    public RuleSet Clone()
    {
        return new()
        {
            Title = Title,
            Description = Description,
            SpriteItemId = SpriteItemId,
            CurrentBubbles = CurrentBubbles,
            FutureBubbles = FutureBubbles,
            FishPredictions = FishPredictions,
            JellyPredictions = JellyPredictions,
            FreezeOnCast = FreezeOnCast,
            RespawnOnCancel = RespawnOnCancel,
        };
    }

    /// <summary>
    /// Copies the rules - <b>excluding</b> metadata such as <see cref="Title"/> and
    /// <see cref="Description"/> - from another instance.
    /// </summary>
    /// <param name="other">The instance to copy from.</param>
    public void CopyFrom(RuleSet other)
    {
        CurrentBubbles = other.CurrentBubbles;
        FutureBubbles = other.FutureBubbles;
        FishPredictions = other.FishPredictions;
        JellyPredictions = other.JellyPredictions;
        FreezeOnCast = other.FreezeOnCast;
        RespawnOnCancel = other.RespawnOnCancel;
    }

    /// <summary>
    /// Prevents the <see cref="Description"/> from being serialized by <c>Newtonsoft.Json</c>.
    /// </summary>
    /// <remarks>
    /// Description should only be read, not written. It is only used by built-in rule sets and has
    /// no meaning in configuration data.
    /// </remarks>
    /// <returns><c>false</c></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Must be public to be seen by Newtonsoft.Json"
    )]
    public bool ShouldSerializeDescription()
    {
        return false;
    }

    /// <summary>
    /// Prevents the <see cref="SpriteItemId"/> from being serialized by <c>Newtonsoft.Json</c>.
    /// </summary>
    /// <remarks>
    /// Sprite item ID should only be read, not written. It is only used by built-in rule sets and
    /// has no meaning in configuration data.
    /// </remarks>
    /// <returns><c>false</c></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Must be public to be seen by Newtonsoft.Json"
    )]
    public bool ShouldSerializeSpriteItemId()
    {
        return false;
    }

    /// <summary>
    /// Prevents the <see cref="Title"/> from being serialized by <c>Newtonsoft.Json</c>.
    /// </summary>
    /// <remarks>
    /// Title should only be read, not written. It is only used by built-in rule sets and has no
    /// meaning in configuration data.
    /// </remarks>
    /// <returns><c>false</c></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Must be public to be seen by Newtonsoft.Json"
    )]
    public bool ShouldSerializeTitle()
    {
        return false;
    }
}
