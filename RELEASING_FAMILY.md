# Coordinated family releases

The dispatcher family is released from seven independent repositories:

- `GuiDispatcher.Sharp`
- `GuiDispatcher.Sharp.Avalonia`
- `GuiDispatcher.Sharp.Consolonia`
- `GuiDispatcher.Sharp.Maui`
- `GuiDispatcher.Sharp.ProGPU`
- `GuiDispatcher.Sharp.Wpf`
- `GuiDispatcher.Sharp.WinUI`

All stable packages in a family release use the same version. Adapter
dependencies have their `GuiDispatcher.Sharp` minimum set to that version.
The ProGPU adapter uses the same stable base plus a suffix matching the upstream
preview, for example `1.1.1-preview26` for `ProGPU.WinUI`
`0.1.0-preview.26`.

## One-time NuGet setup

Each repository publishes through GitHub OIDC and `NuGet/login@v1`. No stored
NuGet API key is required.

Create or verify one trusted publishing policy for each repository on
nuget.org. Every policy uses:

- repository owner: `buchmiet`
- workflow file: `publish-nuget.yml`
- environment: `production`

The repository field must match the package repository:

| Package | Repository |
|---------|------------|
| `GuiDispatcher.Sharp` | `GuiDispatcher.Sharp` |
| `GuiDispatcher.Sharp.Avalonia` | `GuiDispatcher.Sharp.Avalonia` |
| `GuiDispatcher.Sharp.Consolonia` | `GuiDispatcher.Sharp.Consolonia` |
| `GuiDispatcher.Sharp.Maui` | `GuiDispatcher.Sharp.Maui` |
| `GuiDispatcher.Sharp.ProGPU` | `GuiDispatcher.Sharp.ProGPU` |
| `GuiDispatcher.Sharp.Wpf` | `GuiDispatcher.Sharp.Wpf` |
| `GuiDispatcher.Sharp.WinUI` | `GuiDispatcher.Sharp.WinUI` |

The GitHub `production` environment must also exist in every repository.

## Release order

NuGet does not provide an atomic multi-package publish operation. A coordinated
release therefore runs in two waves:

1. Push the common version tag to `GuiDispatcher.Sharp`.
2. Wait until that core version appears in the NuGet V3 index.
3. Push the stable tag to the stable adapters and the corresponding preview tag
   to ProGPU. Their independent GitHub Actions workflows then run at
   approximately the same time.

This order is required because each adapter has a dependency floor matching the
family release version.

## Automated release

Before starting, ensure every repository:

- is on a clean `main` branch;
- has the intended version in its project file;
- has a matching `CHANGELOG.md` entry;
- has committed and pushed release content;
- has no existing tag for that version.

Validate without changing GitHub or NuGet:

```powershell
pwsh ./tools/release-family.ps1 -Version 1.1.1 -ProGpuSuffix preview26 -WhatIf
```

Start the release:

```powershell
pwsh ./tools/release-family.ps1 -Version 1.1.1 -ProGpuSuffix preview26
```

The script validates all seven repositories, pushes their `main` branches,
publishes the core tag, waits for NuGet indexing, and then publishes the adapter
tags as one wave. In this example the stable repositories receive `v1.1.1`,
while ProGPU receives `v1.1.1-preview26`.

Afterward, verify all seven GitHub Actions runs and all package versions on
nuget.org. If one adapter workflow fails, rerun that workflow; do not create a
different package version for only one adapter.
