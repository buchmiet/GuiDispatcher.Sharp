[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'Medium')]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$')]
    [string] $Version,

    [ValidatePattern('^preview\d+$')]
    [string] $ProGpuSuffix = 'preview26',

    [string] $Root = (Join-Path (Split-Path $PSScriptRoot -Parent) '..'),

    [ValidateRange(1, 60)]
    [int] $NuGetTimeoutMinutes = 20
)

$ErrorActionPreference = 'Stop'
$rootPath = (Resolve-Path -LiteralPath $Root).Path

$repositoryNames = @(
    'GuiDispatcher.Sharp',
    'GuiDispatcher.Sharp.Avalonia',
    'GuiDispatcher.Sharp.Consolonia',
    'GuiDispatcher.Sharp.Maui',
    'GuiDispatcher.Sharp.ProGPU',
    'GuiDispatcher.Sharp.Wpf',
    'GuiDispatcher.Sharp.WinUI'
)

function Invoke-RepositoryGit {
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryPath,

        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    $output = & git -C $RepositoryPath @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed in '$RepositoryPath':`n$($output -join "`n")"
    }

    return $output
}

function Test-RemoteTag {
    param(
        [Parameter(Mandatory)]
        [string] $RepositoryPath,

        [Parameter(Mandatory)]
        [string] $Tag
    )

    $output = & git -C $RepositoryPath ls-remote --exit-code --tags origin "refs/tags/$Tag" 2>$null
    if ($LASTEXITCODE -eq 0) {
        return $true
    }

    if ($LASTEXITCODE -eq 2) {
        return $false
    }

    throw "Could not inspect remote tags in '$RepositoryPath'."
}

function Publish-RepositoryTag {
    param(
        [Parameter(Mandatory)]
        [pscustomobject] $Repository,

        [Parameter(Mandatory)]
        [System.Management.Automation.PSCmdlet] $CommandContext
    )

    if (Test-RemoteTag -RepositoryPath $Repository.Path -Tag $Repository.Tag) {
        Write-Host "$($Repository.Name): remote tag $($Repository.Tag) already exists; leaving it unchanged."
        return
    }

    $localTag = Invoke-RepositoryGit -RepositoryPath $Repository.Path -Arguments @(
        'tag', '--list', $Repository.Tag
    )

    if ($localTag) {
        $tagCommit = Invoke-RepositoryGit -RepositoryPath $Repository.Path -Arguments @(
            'rev-list', '-n', '1', $Repository.Tag
        )
        $headCommit = Invoke-RepositoryGit -RepositoryPath $Repository.Path -Arguments @(
            'rev-parse', 'HEAD'
        )

        if ($tagCommit -ne $headCommit) {
            throw "$($Repository.Name): local tag $($Repository.Tag) does not point to HEAD."
        }
    }

    if ($CommandContext.ShouldProcess("$($Repository.Name) at $($Repository.Commit)", "push tag $($Repository.Tag)")) {
        if (-not $localTag) {
            Invoke-RepositoryGit -RepositoryPath $Repository.Path -Arguments @(
                'tag', '-a', $Repository.Tag, '-m', "Release $($Repository.Version)"
            ) | Out-Null
        }

        Invoke-RepositoryGit -RepositoryPath $Repository.Path -Arguments @(
            'push', 'origin', $Repository.Tag
        ) | Out-Null
        Write-Host "$($Repository.Name): pushed $($Repository.Tag)."
    }
}

$repositories = foreach ($name in $repositoryNames) {
    $path = Join-Path $rootPath $name
    if (-not (Test-Path -LiteralPath (Join-Path $path '.git'))) {
        throw "Missing git repository: $path"
    }

    $branch = Invoke-RepositoryGit -RepositoryPath $path -Arguments @(
        'branch', '--show-current'
    )
    if ($branch -ne 'main') {
        throw "$name must be on main; current branch is '$branch'."
    }

    $status = Invoke-RepositoryGit -RepositoryPath $path -Arguments @(
        'status', '--porcelain'
    )
    if ($status) {
        throw "$name has uncommitted changes."
    }

    $remote = Invoke-RepositoryGit -RepositoryPath $path -Arguments @(
        'remote', 'get-url', 'origin'
    )

    $repositoryVersion = if ($name -eq 'GuiDispatcher.Sharp.ProGPU') {
        "$Version-$ProGpuSuffix"
    }
    else {
        $Version
    }

    $projectPath = Join-Path $path "$name.csproj"
    [xml] $project = Get-Content -Raw -LiteralPath $projectPath
    $projectVersion = $project.SelectSingleNode(
        '/Project/PropertyGroup/Version'
    ).InnerText
    if ($projectVersion -ne $repositoryVersion) {
        throw "$name has version $projectVersion; expected $repositoryVersion."
    }

    $changelog = Get-Content -Raw -LiteralPath (Join-Path $path 'CHANGELOG.md')
    if ($changelog -notmatch "(?m)^## \[$([Regex]::Escape($repositoryVersion))\](?: -|$)") {
        throw "$name has no CHANGELOG.md entry for $repositoryVersion."
    }

    if ($name -ne 'GuiDispatcher.Sharp') {
        $dependency = $project.SelectSingleNode(
            "/Project/ItemGroup/PackageReference[@Include='GuiDispatcher.Sharp']"
        )
        $expectedRange = "[$Version,2.0.0)"
        if ($null -eq $dependency -or $dependency.Version -ne $expectedRange) {
            throw "$name must depend on GuiDispatcher.Sharp $expectedRange."
        }
    }

    if ($name -eq 'GuiDispatcher.Sharp.ProGPU') {
        $previewNumber = $ProGpuSuffix.Substring('preview'.Length)
        $proGpuDependency = $project.SelectSingleNode(
            "/Project/ItemGroup/PackageReference[@Include='ProGPU.WinUI']"
        )
        $expectedProGpuRange = "[0.1.0-preview.$previewNumber,0.2.0)"
        if ($null -eq $proGpuDependency -or $proGpuDependency.Version -ne $expectedProGpuRange) {
            throw "$name must depend on ProGPU.WinUI $expectedProGpuRange."
        }
    }

    [pscustomobject]@{
        Name = $name
        Path = $path
        Remote = $remote
        Version = $repositoryVersion
        Tag = "v$repositoryVersion"
        Commit = Invoke-RepositoryGit -RepositoryPath $path -Arguments @(
            'rev-parse', 'HEAD'
        )
    }
}

Write-Host "Validated $($repositories.Count) repositories for family version $Version (ProGPU $Version-$ProGpuSuffix)."

foreach ($repository in $repositories) {
    if ($PSCmdlet.ShouldProcess("$($repository.Name)/main", "push to origin")) {
        Invoke-RepositoryGit -RepositoryPath $repository.Path -Arguments @(
            'push', 'origin', 'main'
        ) | Out-Null
        Write-Host "$($repository.Name): main is pushed."
    }
}

$core = $repositories | Where-Object Name -eq 'GuiDispatcher.Sharp'
Publish-RepositoryTag -Repository $core -CommandContext $PSCmdlet

if (-not $WhatIfPreference) {
    $packageIndex = "https://api.nuget.org/v3-flatcontainer/guidispatcher.sharp/index.json"
    $deadline = [DateTimeOffset]::UtcNow.AddMinutes($NuGetTimeoutMinutes)

    Write-Host "Waiting for GuiDispatcher.Sharp $Version in the NuGet V3 index..."
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        try {
            $versions = (Invoke-RestMethod -Uri $packageIndex).versions
            if ($versions -contains $Version.ToLowerInvariant()) {
                Write-Host "GuiDispatcher.Sharp $Version is visible on NuGet."
                break
            }
        }
        catch {
            Write-Verbose "NuGet index is not ready: $($_.Exception.Message)"
        }

        Start-Sleep -Seconds 15
    }

    if ($versions -notcontains $Version.ToLowerInvariant()) {
        throw "GuiDispatcher.Sharp $Version did not appear on NuGet within $NuGetTimeoutMinutes minutes."
    }
}

$repositories |
    Where-Object Name -ne 'GuiDispatcher.Sharp' |
    ForEach-Object { Publish-RepositoryTag -Repository $_ -CommandContext $PSCmdlet }

if ($WhatIfPreference) {
    Write-Host "What-if validation completed; no branches or tags were pushed."
}
else {
    Write-Host "Family release $Version has been submitted."
}
