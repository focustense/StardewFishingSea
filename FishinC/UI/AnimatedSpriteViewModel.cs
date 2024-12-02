using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;

namespace FishinC.UI;

/// <summary>
/// View model for an animated sprite, using several frames within a single image.
/// </summary>
/// <param name="texture">The source texture in which all frames can be found.</param>
/// <param name="frames">List of unique frames that participate in the animation.</param>
/// <param name="frameDuration">Duration of each frame, i.e. to determine animation speed.</param>
/// <param name="frameOrder">The actual indices of <paramref name="frames"/> to cycle through when
/// advancing; can be <c>null</c> to play the exact <paramref name="frames"/> in their original
/// sequence, but can also omit, repeat or shuffle frames.</param>
/// <param name="startDelay">Delay to wait before advancing from the first frame to subsequent
/// frames. Only affects the first frame of each animation cycle, and is added to the
/// <paramref name="frameDuration"/> for that frame.</param>
public partial class AnimatedSpriteViewModel(
    Texture2D texture,
    IReadOnlyList<Rectangle> frames,
    TimeSpan frameDuration,
    IReadOnlyList<int>? frameOrder = null,
    TimeSpan startDelay = default
) : INotifyPropertyChanged
{
    /// <summary>
    /// The source texture in which all frames can be found.
    /// </summary>
    public Texture2D Texture { get; } = texture;

    private readonly int frameCount = frameOrder?.Count ?? frames.Count;

    /// <summary>
    /// The bounding region of the current frame.
    /// </summary>
    [Notify]
    private Rectangle sourceRect = frameOrder?.Count > 0 ? frames[frameOrder[0]] : frames[0];

    private TimeSpan elapsed;
    private int frameOrderIndex;

    /// <summary>
    /// Runs on every update tick.
    /// </summary>
    /// <param name="elapsed">Time elapsed since last update.</param>
    public void Update(TimeSpan elapsed)
    {
        this.elapsed += elapsed;
        bool isNewFrame = false;
        while (true)
        {
            var currentFrameDuration =
                frameOrderIndex == 0 ? frameDuration + startDelay : frameDuration;
            if (this.elapsed < currentFrameDuration)
            {
                break;
            }
            isNewFrame = true;
            frameOrderIndex++;
            this.elapsed -= currentFrameDuration;
        }
        if (!isNewFrame)
        {
            return;
        }
        frameOrderIndex %= frameCount;
        int frameIndex = frameOrder is not null ? frameOrder[frameOrderIndex] : frameOrderIndex;
        SourceRect = frames[frameIndex];
    }
}
