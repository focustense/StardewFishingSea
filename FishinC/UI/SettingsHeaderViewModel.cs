using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;

namespace FishinC.UI;

/// <summary>
/// View model for the header of the Settings menu.
/// </summary>
public class SettingsHeaderViewModel : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Animation frames and state for the Giant Bait.
    /// </summary>
    public AnimatedSpriteViewModel GiantBait { get; } =
        new(Game1.mouseCursors2, GiantBaitFrames, TimeSpan.FromMilliseconds(500));

    /// <summary>
    /// Animation frames and state for the Mermaid.
    /// </summary>
    public AnimatedSpriteViewModel Mermaid { get; } =
        new(
            TemporarySprites1,
            MermaidFrames,
            TimeSpan.FromMilliseconds(150),
            [0, 1, 2, 1, 2, 3, 4],
            TimeSpan.FromSeconds(4)
        );

    /// <summary>
    /// Reference to the TemporarySprites sprite sheet texture.
    /// </summary>
    private static Texture2D TemporarySprites1 =>
        Game1.content.Load<Texture2D>(@"LooseSprites\temporary_sprites_1");

    private static readonly Rectangle[] GiantBaitFrames =
    [
        new(192, 61, 32, 32),
        new(224, 61, 32, 32),
    ];

    private static readonly Rectangle[] MermaidFrames = Enumerable
        .Range(0, 7)
        .Select(i => new Rectangle(304 + 28 * i, 592, 28, 35))
        .ToArray();

    /// <summary>
    /// Initializes a new <see cref="SettingsHeaderViewModel"/> instance.
    /// </summary>
    public SettingsHeaderViewModel()
    {
        GiantBait.PropertyChanged += AnimateSprite_PropertyChanged;
        Mermaid.PropertyChanged += AnimateSprite_PropertyChanged;
    }

    /// <summary>
    /// Runs on every update tick.
    /// </summary>
    /// <param name="elapsed">Time elapsed since last update.</param>
    public void Update(TimeSpan elapsed)
    {
        // Forward these since the view never binds the individual sprites as context data, and
        // therefore they won't be automatically updated.
        GiantBait.Update(elapsed);
        Mermaid.Update(elapsed);
    }

    private void AnimateSprite_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // We have to do this because the view is binding to the entire sprite (GiantBait or
        // Mermaid), not to its individual properties. So even though the AnimatedSpriteViewModel
        // implements INPC, StardewUI won't associate that with the GiantBait or Mermaid property
        // on THIS view model changing; we have to forward it manually.
        if (sender == GiantBait)
        {
            OnPropertyChanged(nameof(GiantBait));
        }
        else if (sender == Mermaid)
        {
            OnPropertyChanged(nameof(Mermaid));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
