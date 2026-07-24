# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.1] - 2026-07-24

### Added

- Tests for immediate dispatch, asynchronous invocation, repeating timers, and one-shot scheduling.
- Test execution as a required step of the NuGet publishing workflow.

### Changed

- Excluded test sources and assets from the library project and package.
- Documented the Avalonia, Consolonia, .NET MAUI, and WPF adapter packages.

## [1.1.0] - 2026-07-10

### Changed

- Target framework is now `net10.0` (dropped `netstandard2.0`).
- Public contracts (`IGuiDispatcher`, `IGuiTimer`) live in the `GuiDispatcher.Sharp.Contracts` namespace.
- `ImmediateGuiDispatcher` timer implementations are `internal`; consumers use `IGuiTimer` via `CreateTimer` and `RunOnce`.
- Removed `sealed` from public types.
- Argument validation uses `ArgumentNullException.ThrowIfNull`.

### Added

- Source layout: `Contracts/`, `Timers/`, and `Static/` folders.

## [1.0.2] - 2026-07-01

### Changed

- Switched releases to a git-tag-triggered flow (`vX.Y.Z`) instead of publishing on every push to `main`. The `Publish NuGet` workflow now validates that the `.csproj` version and the tag match, and that `CHANGELOG.md` has a corresponding entry, before packing and pushing to NuGet.

## [1.0.1] - 2026-07-01

### Added

- Initial public release of `GuiDispatcher.Sharp`.
- `IGuiDispatcher` abstraction: `CheckAccess`, `Post`, `Invoke`, `InvokeAsync`, `Invoke<T>`, `CreateTimer`, `RunOnce`.
- `IGuiTimer` abstraction: `Tick`, `Interval`, `IsEnabled`, `Start`, `Stop`.
- `ImmediateGuiDispatcher`: headless/default implementation for tests and console hosts.
