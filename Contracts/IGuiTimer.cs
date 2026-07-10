namespace GuiDispatcher.Sharp.Contracts;

/// <summary>A dispatcher-backed timer abstraction.</summary>
public interface IGuiTimer : IDisposable
{
    /// <summary>Raised on each tick when the timer is enabled.</summary>
    event EventHandler? Tick;

    /// <summary>Interval between ticks.</summary>
    TimeSpan Interval { get; set; }

    /// <summary>True while the timer is running.</summary>
    bool IsEnabled { get; set; }

    /// <summary>Starts the timer.</summary>
    void Start();

    /// <summary>Stops the timer.</summary>
    void Stop();
}
