namespace GuiDispatcher.Sharp;

/// <summary>
/// Marshals work onto a GUI/main thread. Test and headless hosts can use
/// <see cref="ImmediateGuiDispatcher"/>, which executes work inline.
/// </summary>
public interface IGuiDispatcher
{
    /// <summary>True when the caller already runs on the GUI/main thread.</summary>
    bool CheckAccess();

    /// <summary>Queues work and returns immediately.</summary>
    void Post(Action action);

    /// <summary>Runs work on the GUI/main thread and blocks until it completes.</summary>
    void Invoke(Action action);

    /// <summary>Runs work on the GUI/main thread and completes when it finishes.</summary>
    Task InvokeAsync(Action action);

    /// <summary>Runs asynchronous work on the GUI/main thread and completes when the returned task finishes.</summary>
    Task InvokeAsync(Func<Task> action);

    /// <summary>Runs work on the GUI/main thread and returns its result.</summary>
    T Invoke<T>(Func<T> func);

    /// <summary>Creates a timer whose ticks are delivered through this dispatcher.</summary>
    IGuiTimer CreateTimer(TimeSpan interval);

    /// <summary>Schedules work to run once after the specified interval.</summary>
    IDisposable RunOnce(Action action, TimeSpan interval);
}
