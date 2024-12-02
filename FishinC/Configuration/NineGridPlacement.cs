namespace FishinC.Configuration;

/// <summary>
/// Model for content placement along a nine-segment grid, i.e. all possible combinations of horizontal and vertical
/// <see cref="Alignment"/>.
/// </summary>
/// <remarks>
/// Copied from StardewUI and used to align HUD.
/// </remarks>
/// <param name="HorizontalAlignment">Content alignment along the horizontal axis.</param>
/// <param name="VerticalAlignment">Content alignment along the vertical axis.</param>
/// <param name="Offset">Absolute axis-independent pixel offset.</param>
[DuckType]
public record NineGridPlacement(
    Alignment HorizontalAlignment,
    Alignment VerticalAlignment,
    Point Offset = default
)
{
    /// <summary>
    /// Computes the position of this placement within a given viewport.
    /// </summary>
    /// <param name="size">The viewport size.</param>
    public Vector2 GetPosition(Vector2 size)
    {
        var x = HorizontalAlignment switch
        {
            Alignment.Middle => size.X / 2,
            Alignment.End => size.X,
            _ => 0,
        };
        var y = VerticalAlignment switch
        {
            Alignment.Middle => size.Y / 2,
            Alignment.End => size.Y,
            _ => 0,
        };
        return new(x, y);
    }
}
