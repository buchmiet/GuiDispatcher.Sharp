# GuiDispatcher.Sharp

Platform-agnostic GUI dispatcher and timer abstractions for **.NET 10**.

Use this package from view models, services, debouncers, and other code that needs to marshal work back to a GUI/main thread without referencing a UI framework directly.

## Install

```xml
<PackageReference Include="GuiDispatcher.Sharp" Version="1.1.*" />
```

Or via CLI:

```bash
dotnet add package GuiDispatcher.Sharp
```

Versioning follows [Semantic Versioning](https://semver.org/). Releases are cut by pushing a `vX.Y.Z` git tag; see [CHANGELOG.md](CHANGELOG.md) for release history.

## Requirements

- .NET 10 (`net10.0`)

## NuGet publishing

Publishing uses NuGet Trusted Publishing from GitHub Actions, not a stored API key.

Configure a trusted publishing policy on nuget.org:

| Field | Value |
|-------|-------|
| Repository owner | `buchmiet` |
| Repository | `GuiDispatcher.Sharp` |
| Workflow file | `publish-nuget.yml` |
| Environment | `production` |

### Releasing

The `Publish NuGet` workflow only runs on `vX.Y.Z` tag pushes, and will fail unless the `.csproj` version and `CHANGELOG.md` are both updated first. To cut a release:

1. Bump `<Version>` in `GuiDispatcher.Sharp.csproj`.
2. Move the relevant `[Unreleased]` entries in `CHANGELOG.md` into a new `## [X.Y.Z] - YYYY-MM-DD` section.
3. Commit both changes.
4. Tag and push: `git tag vX.Y.Z && git push origin vX.Y.Z` — this triggers the publish workflow.

## Contracts

Interfaces are in the `GuiDispatcher.Sharp.Contracts` namespace:

```csharp
using GuiDispatcher.Sharp.Contracts;

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
using GuiDispatcher.Sharp.Contracts;

IGuiDispatcher dispatcher = new ImmediateGuiDispatcher();

await dispatcher.InvokeAsync(() =>
{
    viewModel.Apply(result);
});
```

`ImmediateGuiDispatcher` executes posted/invoked work inline. Its timers use `System.Threading.Timer` and marshal ticks through `Post`, which keeps tests and console hosts deterministic enough without a GUI framework dependency.

## UI implementations

Install the adapter package matching the UI host:

```xml
<PackageReference Include="GuiDispatcher.Sharp.Avalonia" Version="1.1.*" />
<PackageReference Include="GuiDispatcher.Sharp.Consolonia" Version="1.1.*" />
<PackageReference Include="GuiDispatcher.Sharp.Wpf" Version="1.1.*" />
```

- `GuiDispatcher.Sharp.Avalonia` targets Avalonia 12.
- `GuiDispatcher.Sharp.Consolonia` targets Consolonia 11 and its Avalonia 11 dispatcher.
- `GuiDispatcher.Sharp.Wpf` targets WPF on .NET 10 for Windows.

WinUI 3 support is planned separately.
