using StardewUI;

namespace FishingBuddy.UI;

/// <summary>
/// Content-agnostic <see cref="SpeechBubble{T}"/> container using <see cref="IView"/> as inner type.
/// </summary>
public class SpeechBubble : SpeechBubble<IView> { }

/// <summary>
/// Presents content inside the comic-style speech bubble, automatically resizing the bubble to fit
/// the inner content size.
/// </summary>
/// <typeparam name="T">Type of content view.</typeparam>
public class SpeechBubble<T> : WrapperView
    where T : IView
{
    /// <summary>
    /// Maximum vertical distance, in pixels, for the animated bouncing (up and down).
    /// </summary>
    public float BounceAmplitude { get; set; } = 4f;

    /// <summary>
    /// Duration of an animated bounce before returning to base position.
    /// </summary>
    public float BounceDurationMs { get; set; } = 250f;

    /// <summary>
    /// The inner content to display inside the bubble.
    /// </summary>
    public T? Content
    {
        get => contentView;
        set
        {
            contentView = value;
            if (contentPanel is not null)
            {
                contentPanel.Children = value is not null ? [value] : [];
            }
        }
    }

    private Panel? contentPanel;
    private T? contentView;

    public override void Draw(ISpriteBatch b)
    {
        float yOffset =
            BounceAmplitude
            * (float)
                Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / BounceDurationMs);
        b.Translate(0, yOffset);
        base.Draw(b);
    }

    protected override IView CreateView()
    {
        var bodyImage = new Image()
        {
            Layout = new()
            {
                Width = Length.Stretch(),
                Height = Length.Stretch(),
                MinHeight = 60.0f,
            },
            Fit = ImageFit.Stretch,
            Sprite = Sprites.SpeechBubbleBody,
        };
        contentPanel = new Panel()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = new(12),
            Padding = new(Bottom: 4),
            Children = contentView is not null ? [contentView] : [],
        };
        var bodyPanel = new Panel()
        {
            Layout = LayoutParameters.FitContent(),
            VerticalContentAlignment = Alignment.Middle,
            Children = [bodyImage, contentPanel],
        };
        var closerImage = new Image()
        {
            Layout = new() { Width = Length.Stretch(), Height = Length.Px(4) },
            Margin = new(Left: 20, Right: 20, Top: -4),
            Fit = ImageFit.Stretch,
            Sprite = Sprites.SpeechBubbleCloser,
        };
        var tailImage = new Image()
        {
            Layout = new() { Width = Length.Stretch(), Height = Length.Content() },
            Margin = new(Top: -4),
            Fit = ImageFit.None,
            HorizontalAlignment = Alignment.Middle,
            Sprite = Sprites.SpeechBubbleTail,
        };
        return new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            Orientation = Orientation.Vertical,
            Children = [bodyPanel, closerImage, tailImage],
        };
    }
}
