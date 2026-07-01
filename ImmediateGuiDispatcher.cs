namespace GuiDispatcher.Sharp;

/// <summary>
/// Dispatcher for tests, console tools, and other headless hosts. Work runs inline
/// on the caller, while timers use the thread pool and marshal ticks through
/// <see cref="Post"/>.
/// </summary>
public sealed class ImmediateGuiDispatcher : IGuiDispatcher
{
    public bool CheckAccess() => true;

    public void Post(Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

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
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        return action();
    }

    public T Invoke<T>(Func<T> func)
    {
        if (func is null)
            throw new ArgumentNullException(nameof(func));

        return func();
    }

    public IGuiTimer CreateTimer(TimeSpan interval) => new ThreadPoolGuiTimer(this, interval);

    public IDisposable RunOnce(Action action, TimeSpan interval)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        return new OneShotTimer(this, action, NormalizeInterval(interval));
    }

    private static TimeSpan NormalizeInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than or equal to zero.");

        return interval == TimeSpan.Zero ? TimeSpan.FromTicks(1) : interval;
    }

    private sealed class ThreadPoolGuiTimer : IGuiTimer
    {
        private readonly ImmediateGuiDispatcher _dispatcher;
        private readonly object _gate = new();
        private Timer? _timer;
        private TimeSpan _interval;
        private bool _disposed;

        public ThreadPoolGuiTimer(ImmediateGuiDispatcher dispatcher, TimeSpan interval)
        {
            _dispatcher = dispatcher;
            _interval = NormalizeInterval(interval);
        }

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
                var normalized = NormalizeInterval(value);
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

                _timer = new Timer(_ => _dispatcher.Post(() => Tick?.Invoke(this, EventArgs.Empty)), null, _interval, _interval);
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

    private sealed class OneShotTimer : IDisposable
    {
        private readonly ImmediateGuiDispatcher _dispatcher;
        private readonly Action _action;
        private readonly object _gate = new();
        private Timer? _timer;
        private bool _disposed;

        public OneShotTimer(ImmediateGuiDispatcher dispatcher, Action action, TimeSpan interval)
        {
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
}
