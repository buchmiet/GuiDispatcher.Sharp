using GuiDispatcher.Sharp;

namespace GuiDispatcher.Sharp.Tests;

public class ImmediateGuiDispatcherRunOnceTests
{
    [Fact]
    public void RunOnce_NullAction_ThrowsArgumentNullException()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        Assert.Throws<ArgumentNullException>(() => dispatcher.RunOnce(null!, TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public void RunOnce_NegativeInterval_Throws()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        Assert.Throws<ArgumentOutOfRangeException>(() => dispatcher.RunOnce(() => { }, TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public async Task RunOnce_Dispose_PreventsExecution()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        var executed = false;
        using (dispatcher.RunOnce(() => executed = true, TimeSpan.FromMilliseconds(500)))
        {
        }

        await Task.Delay(700);
        Assert.False(executed);
    }

    [Fact]
    public async Task RunOnce_ExecutesOnceAfterDelay()
    {
        var dispatcher = new ImmediateGuiDispatcher();
        var executed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var executeCount = 0;
        using var _ = dispatcher.RunOnce(() =>
        {
            Interlocked.Increment(ref executeCount);
            executed.TrySetResult();
        }, TimeSpan.FromMilliseconds(50));

        await executed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Task.Delay(100);
        Assert.Equal(1, executeCount);
    }
}
