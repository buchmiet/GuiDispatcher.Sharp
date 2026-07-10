using GuiDispatcher.Sharp.Contracts;
using GuiDispatcher.Sharp.Static;

namespace GuiDispatcher.Sharp;

/// <summary>
/// Dispatcher for tests, console tools, and other headless hosts. Work runs inline
/// on the caller, while timers use the thread pool and marshal ticks through
/// <see cref="Post"/>.
/// </summary>
public class ImmediateGuiDispatcher : IGuiDispatcher
{
    public bool CheckAccess() => true;

    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
    }

    public void Invoke(Action action) => Post(action);

    public Task InvokeAsync(Action action)
    {
        Post(action);
        return Task.CompletedTask;
    }

    public Task InvokeAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return action();
    }

    public T Invoke<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return func();
    }

    public IGuiTimer CreateTimer(TimeSpan interval) => new Timers.ThreadPoolGuiTimer(this, interval);

    public IDisposable RunOnce(Action action, TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new Timers.OneShotTimer(this, action, Normalizations.NormalizeInterval(interval));
    }
}
