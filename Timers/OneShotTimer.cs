namespace GuiDispatcher.Sharp.Timers;

internal class OneShotTimer : IDisposable
{
    private readonly ImmediateGuiDispatcher _dispatcher;
    private readonly Action _action;
    private readonly Lock _gate = new();
    private Timer? _timer;
    private bool _disposed;

    public OneShotTimer(ImmediateGuiDispatcher dispatcher, Action action, TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(action);
        _dispatcher = dispatcher;
        _action = action;
        _timer = new Timer(OnTimer, null, interval, Timeout.InfiniteTimeSpan);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            _timer?.Dispose();
            _timer = null;
        }
    }

    private void OnTimer(object? state)
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            _timer?.Dispose();
            _timer = null;
        }

        _dispatcher.Post(_action);
    }
}
