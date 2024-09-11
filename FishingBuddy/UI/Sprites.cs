using Microsoft.Xna.Framework.Graphics;
using StardewUI;
using StardewValley.ItemTypeDefinitions;

namespace FishingBuddy.UI;

/// <summary>
/// Sprites used in the mod UI (including overlays).
/// </summary>
internal static class Sprites
{
    private static Texture2D TemporarySprites1 =>
        Game1.content.Load<Texture2D>(@"LooseSprites\temporary_sprites_1");

    /// <summary>
    /// Default frame of the standard bobber used in the fishing minigame.
    /// </summary>
    public static Sprite BobberDefault => new(Game1.mouseCursors, new(170, 1903, 7, 8));

    /// <summary>
    /// Generic fish icon shown during fishing minigame.
    /// </summary>
    public static Sprite GenericFish => new(Game1.mouseCursors, new(615, 1841, 19, 19));

    /// <summary>
    /// Animation frames for the giant bait sprite used in a certain quest.
    /// </summary>
    public static IReadOnlyList<Sprite> GiantBait =>
        [
            new(Game1.mouseCursors2, new(192, 61, 32, 32)),
            new(Game1.mouseCursors2, new(224, 61, 32, 32)),
        ];

    /// <summary>
    /// Animation frames for the mermaid character without a shadow.
    /// </summary>
    public static IReadOnlyList<Sprite> Mermaid =>
        Enumerable
            .Range(0, 7)
            .Select(i => new Sprite(TemporarySprites1, new(304 + 28 * i, 592, 28, 35)))
            .ToList();

    /// <summary>
    /// Semi-rounded frame that looks like a drawer pulled out from the <see cref="OverlayFrame"/>.
    /// </summary>
    public static Sprite OverlayDrawer =>
        new(
            Game1.mouseCursors,
            SourceRect: new(317, 361, 12, 22),
            FixedEdges: new(1, 7, 6, 4),
            SliceSettings: new(Scale: 4)
        );

    /// <summary>
    /// Frame used for game-world overlays; very similar to <see cref="UiSprites.ControlBorder"/>
    /// but more uniform and without shadows.
    /// </summary>
    public static Sprite OverlayFrame =>
        new(
            Game1.mouseCursors,
            SourceRect: new(293, 360, 24, 24),
            FixedEdges: new(5),
            SliceSettings: new(Scale: 4)
        );

    /// <summary>
    /// Red rectangular border indicating a selected item or widget.
    /// </summary>
    public static Sprite SelectionBorder =>
        new(
            Game1.menuTexture,
            SourceRect: new(0, 896, 64, 64),
            FixedEdges: new(4),
            SliceSettings: new(EdgesOnly: true)
        );

    /// <summary>
    /// The main body of the comic-style speech bubble which is partially open at the bottom.
    /// </summary>
    public static Sprite SpeechBubbleBody =>
        new(
            Game1.mouseCursors,
            SourceRect: new(141, 465, 20, 20),
            FixedEdges: new(5),
            SliceSettings: new(Scale: 4)
        );

    /// <summary>
    /// A small segment of the <see cref="SpeechBubbleBody"/> that can be stretched to close the
    /// bottom segment.
    /// </summary>
    /// <remarks>
    /// The tail is then drawn on top of the stretched segment to "reopen" it.
    /// </remarks>
    public static Sprite SpeechBubbleCloser =>
        new(Game1.mouseCursors, SourceRect: new(145, 484, 4, 1));

    /// <summary>
    /// The tail of the speech bubble. Needs to be drawn unstretched, except for the base scale.
    /// </summary>
    public static Sprite SpeechBubbleTail =>
        new(Game1.mouseCursors, SourceRect: new(141, 484, 20, 5), SliceSettings: new(Scale: 4));

    /// <summary>
    /// Border/background sprite thinner than <see cref="UiSprites.ControlBorder"/>, typically used
    /// for items.
    /// </summary>
    public static Sprite ThinBorder =>
        new(Game1.menuTexture, SourceRect: new(0, 320, 60, 60), FixedEdges: new(4, 9, 8, 4));

    /// <summary>
    /// Very small (cursor-sized) clock icon shown for e.g. timed quests.
    /// </summary>
    public static Sprite TinyClock => new(Game1.mouseCursors, new(434, 475, 9, 9));

    /// <summary>
    /// Large water area in the Cursors texture.
    /// </summary>
    public static Sprite WaterBackground => new(Game1.mouseCursors, new(0, 2000, 640, 256));

    /// <summary>
    /// Gets a <see cref="Sprite"/> for an in-game item, given its ID.
    /// </summary>
    /// <returns>
    /// The item or error sprite.
    /// </returns>
    public static Sprite Item(string qualifiedItemId)
    {
        var itemData = ItemRegistry.GetDataOrErrorItem(qualifiedItemId);
        return Item(itemData);
    }

    /// <summary>
    /// Gets a <see cref="Sprite"/> for an in-game item, given its item data.
    /// </summary>
    /// <returns>
    /// The item sprite.
    /// </returns>
    public static Sprite Item(ParsedItemData itemData)
    {
        return new(itemData.GetTexture(), SourceRect: itemData.GetSourceRect());
    }
}
