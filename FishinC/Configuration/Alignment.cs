namespace FishinC.Configuration;

/// <summary>
/// Specifies an alignment (horizontal or vertical) for text or other layout.
/// </summary>
/// <remarks>
/// Copied from StardewUI and used to align HUD.
/// </remarks>
public enum Alignment
{
    /// <summary>
    /// Align to the start of the available space - horizontal left or vertical top.
    /// </summary>
    Start,

    /// <summary>
    /// Align to the middle of the available space.
    /// </summary>
    Middle,

    /// <summary>
    /// Align to the end of the available space - horizontal right or vertical bottom.
    /// </summary>
    End,
}
