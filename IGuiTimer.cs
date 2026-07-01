namespace GuiDispatcher.Sharp;

/// <summary>A dispatcher-backed timer abstraction.</summary>
public interface IGuiTimer : IDisposable
{
    event EventHandler? Tick;

    TimeSpan Interval { get; set; }

    bool IsEnabled { get; set; }

    void Start();

    void Stop();
}
