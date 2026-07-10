namespace GuiDispatcher.Sharp.Static;

internal static class Normalizations
{
    internal static TimeSpan NormalizeInterval(TimeSpan interval) => interval < TimeSpan.Zero
        ? throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than or equal to zero.")
        : interval == TimeSpan.Zero ? TimeSpan.FromTicks(1) : interval;
}
