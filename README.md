# GuiDispatcher.Sharp

Small platform-agnostic UI dispatcher and UI timer abstractions.

Use this package from view models, services, debouncers, and other code that needs to marshal work back to a GUI/main thread without referencing a UI framework directly.

## Install

```xml
<PackageReference Include="GuiDispatcher.Sharp" Version="1.0.*" />
```

Or via CLI:

```bash
dotnet add package GuiDispatcher.Sharp
```

Versioning follows CI run numbers (`1.0.{run}`) on pushes to `main`.

## NuGet publishing

Publishing uses NuGet Trusted Publishing from GitHub Actions, not a stored API key.

Configure a trusted publishing policy on nuget.org:

| Field | Value |
|-------|-------|
| Repository owner | `buchmiet` |
| Repository | `GuiDispatcher.Sharp` |
| Workflow file | `publish-nuget.yml` |
| Environment | `production` |

## Contracts

```csharp
public interface IGuiDispatcher
{
    bool CheckAccess();
    void Post(Action action);
    void Invoke(Action action);
    Task InvokeAsync(Action action);
    Task InvokeAsync(Func<Task> action);
    T Invoke<T>(Func<T> func);
    IGuiTimer CreateTimer(TimeSpan interval);
    IDisposable RunOnce(Action action, TimeSpan interval);
}
```

`IGuiTimer` exposes `Tick`, `Interval`, `IsEnabled`, `Start`, `Stop`, and `Dispose`.

## Headless/default implementation

```csharp
using GuiDispatcher.Sharp;

IGuiDispatcher dispatcher = new ImmediateGuiDispatcher();

await dispatcher.InvokeAsync(() =>
{
    viewModel.Apply(result);
});
```

`ImmediateGuiDispatcher` executes posted/invoked work inline. Its timers use `System.Threading.Timer` and dispatch ticks through `Post`, which keeps tests and console hosts deterministic enough without a GUI framework dependency.

## UI implementations

Install a platform adapter package for GUI applications:

```xml
<PackageReference Include="GuiDispatcher.Sharp.Avalonia" Version="1.0.*" />
```

WinUI 3 support is planned separately.
