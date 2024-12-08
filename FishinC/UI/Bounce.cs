namespace FishinC.UI;

/// <summary>
/// Helper model for calculating the current offset of a "bouncing" object, e.g. a speech balloon.
/// </summary>
public class Bounce
{
    /// <summary>
    /// Maximum vertical distance, in pixels, for the animated bouncing (up and down).
    /// </summary>
    public float Amplitude { get; set; } = 4f;

    /// <summary>
    /// Duration of an animated bounce before returning to base position.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// The current offset at which the object should be drawn.
    /// </summary>
    public Vector2 Offset { get; private set; }

    private TimeSpan totalTime;

    /// <summary>
    /// Updates the current <see cref="Offset"/>.
    /// </summary>
    /// <param name="elapsed">Time elapsed since last update.</param>
    public void Update(TimeSpan elapsed)
    {
        totalTime += elapsed;
        float y = Amplitude * (float)Math.Sin(totalTime / Duration);
        Offset = new(0, y);
    }
}
