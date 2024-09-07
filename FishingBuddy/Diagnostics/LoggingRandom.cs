using System.Diagnostics;

namespace FishingBuddy.Diagnostics;

/// <summary>
/// Helper for rooting out calls directly to <see cref="Game1.random"/> that the transpilers of this
/// mod failed to address.
/// </summary>
/// <remarks>
/// <para>
/// This class is never meant to be used in releases. It is strictly a debugging aid for a
/// commonly-encountered problem.
/// </para>
/// <para>
/// To use, set <see cref="Game1.random"/> to an instance of this, then hold the modifier key while
/// performing an action that is supposed to use the <see cref="FishRandom"/>, e.g. walking around
/// different tiles with <see cref="CatchPreview.Enabled"/> and a rod equipped.
/// </para>
/// <para>
/// Since there may be hundreds of random samples per second, logging is gated behind a
/// <paramref name="modifierKey"/> which must be held for logging to occur.
/// </para>
/// </remarks>
/// <param name="inputHelper">SMAPI input helper.</param>
/// <param name="modifierKey">Modifier key to hold in order to enable log output.</param>
/// <param name="monitor">Logger instance for the mod.</param>
/// <param name="traceFilter">Optional predicate to filter the generated stack trace for each log
/// entry; use to eliminate noise when a root cause is suspected but not confirmed.</param>
internal class LoggingRandom(
    IInputHelper inputHelper,
    SButton modifierKey,
    IMonitor monitor,
    Predicate<string>? traceFilter = null
) : Random
{
    public override int Next()
    {
        LogFrame();
        return base.Next();
    }

    public override int Next(int maxValue)
    {
        LogFrame();
        return base.Next(maxValue);
    }

    public override int Next(int minValue, int maxValue)
    {
        LogFrame();
        return base.Next(minValue, maxValue);
    }

    public override void NextBytes(byte[] buffer)
    {
        LogFrame();
        base.NextBytes(buffer);
    }

    public override void NextBytes(Span<byte> buffer)
    {
        LogFrame();
        base.NextBytes(buffer);
    }

    public override double NextDouble()
    {
        LogFrame();
        return base.NextDouble();
    }

    public override long NextInt64()
    {
        LogFrame();
        return base.NextInt64();
    }

    public override long NextInt64(long maxValue)
    {
        LogFrame();
        return base.NextInt64(maxValue);
    }

    public override long NextInt64(long minValue, long maxValue)
    {
        LogFrame();
        return base.NextInt64(minValue, maxValue);
    }

    public override float NextSingle()
    {
        LogFrame();
        return base.NextSingle();
    }

    private void LogFrame()
    {
        if (!inputHelper.IsDown(modifierKey))
        {
            return;
        }
        var trace = new StackTrace(2, true).ToString();
        if (traceFilter?.Invoke(trace) == true)
        {
            return;
        }
        monitor.Log("Game1.random invoked from: " + trace);
    }
}
