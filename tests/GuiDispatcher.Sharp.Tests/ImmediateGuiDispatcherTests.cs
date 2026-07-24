using GuiDispatcher.Sharp;

namespace GuiDispatcher.Sharp.Tests;

public class ImmediateGuiDispatcherTests
{
    private readonly ImmediateGuiDispatcher _dispatcher = new();

    [Fact]
    public void CheckAccess_ReturnsTrue() => Assert.True(_dispatcher.CheckAccess());

    [Fact]
    public void Post_NullAction_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => _dispatcher.Post(null!));

    [Fact]
    public void InvokeAsync_FuncTask_Null_ThrowsArgumentNullException()
    {
        try
        {
            _ = _dispatcher.InvokeAsync((Func<Task>)null!);
            Assert.Fail("Expected ArgumentNullException.");
        }
        catch (ArgumentNullException)
        {
        }
    }

    [Fact]
    public void Invoke_NullFunc_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => _dispatcher.Invoke<int>(null!));

    [Fact]
    public void Post_ExecutesSynchronously()
    {
        var executed = false;
        _dispatcher.Post(() => executed = true);
        Assert.True(executed);
    }

    [Fact]
    public void Invoke_ReturnsValue() => Assert.Equal(42, _dispatcher.Invoke(() => 42));

    [Fact]
    public async Task InvokeAsync_FuncTask_AwaitsInnerTask()
    {
        var innerCompleted = false;

        await _dispatcher.InvokeAsync(async () =>
        {
            await Task.Delay(50);
            innerCompleted = true;
        });

        Assert.True(innerCompleted);
    }
}
