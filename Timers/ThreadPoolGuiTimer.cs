using GuiDispatcher.Sharp.Contracts;

namespace GuiDispatcher.Sharp.Timers;

internal class ThreadPoolGuiTimer(ImmediateGuiDispatcher dispatcher, TimeSpan interval) : IGuiTimer
{
    private readonly Lock _gate = new();
    private Timer? _timer;
    private TimeSpan _interval = Static.Normalizations.NormalizeInterval(interval);
    private bool _disposed;

    public event EventHandler? Tick;

    public TimeSpan Interval
    {
        get
        {
            lock (_gate)
            {
                return _interval;
            }
        }
        set
        {
            var normalized = Static.Normalizations.NormalizeInterval(value);
            lock (_gate)
            {
                ThrowIfDisposed();
                _interval = normalized;
                _timer?.Change(_interval, _interval);
            }
        }
    }

    public bool IsEnabled
    {
        get
        {
            lock (_gate)
            {
                return _timer is not null;
            }
        }
        set
        {
            if (value)
                Start();
            else
                Stop();
        }
    }

    public void Start()
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            if (_timer is not null)
                return;

            _timer = new Timer(_ => dispatcher.Post(() => Tick?.Invoke(this, EventArgs.Empty)), null, _interval, _interval);
        }
    }

    public void Stop()
    {
        lock (_gate)
        {
            _timer?.Dispose();
            _timer = null;
        }
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

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadPoolGuiTimer));
    }
}
