using GuiDispatcher.Sharp;
using GuiDispatcher.Sharp.Contracts;

namespace GuiDispatcher.Sharp.Tests;

public class ImmediateGuiDispatcherTimerTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    [Fact]
    public void CreateTimer_NegativeInterval_Throws()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        Assert.Throws<ArgumentOutOfRangeException>(() => dispatcher.CreateTimer(TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public void CreateTimer_ZeroInterval_NormalizedToOneTick()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        using var timer = dispatcher.CreateTimer(TimeSpan.Zero);
        Assert.Equal(TimeSpan.FromTicks(1), timer.Interval);
    }

    [Fact]
    public async Task CreateTimer_Start_FiresAtLeastOneTick()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        var ticked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timer = dispatcher.CreateTimer(TimeSpan.FromMilliseconds(50));
        timer.Tick += (_, _) => ticked.TrySetResult();
        timer.Start();
        await ticked.Task.WaitAsync(Timeout);
    }

    [Fact]
    public async Task CreateTimer_Stop_PreventsFurtherTicks()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        var tickCount = 0;
        using var timer = dispatcher.CreateTimer(TimeSpan.FromMilliseconds(30));
        timer.Tick += (_, _) => Interlocked.Increment(ref tickCount);
        timer.Start();
        await Task.Delay(80);
        timer.Stop();
        var countAfterStop = tickCount;
        await Task.Delay(80);
        Assert.True(tickCount >= 1);
        Assert.Equal(countAfterStop, tickCount);
    }

    [Fact]
    public void CreateTimer_Dispose_ThrowsOnStart()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        var timer = dispatcher.CreateTimer(TimeSpan.FromMilliseconds(50));
        timer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => timer.Start());
    }

    [Fact]
    public void CreateTimer_Dispose_ThrowsOnIntervalSet()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        var timer = dispatcher.CreateTimer(TimeSpan.FromMilliseconds(50));
        timer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => timer.Interval = TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void CreateTimer_IsEnabled_ReflectsStartStop()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        using var timer = dispatcher.CreateTimer(TimeSpan.FromMilliseconds(50));
        Assert.False(timer.IsEnabled);
        timer.Start();
        Assert.True(timer.IsEnabled);
        timer.Stop();
        Assert.False(timer.IsEnabled);
    }

    [Fact]
    public async Task CreateTimer_IntervalChange_AfterStart_StillTicks()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        var ticked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timer = dispatcher.CreateTimer(TimeSpan.FromMilliseconds(200));
        timer.Tick += (_, _) => ticked.TrySetResult();
        timer.Start();
        timer.Interval = TimeSpan.FromMilliseconds(30);
        await ticked.Task.WaitAsync(Timeout);
    }
}
