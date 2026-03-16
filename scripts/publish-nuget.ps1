[CmdletBinding()]
param(
    [ValidateSet('patch', 'minor', 'major', 'none')]
    [string]$Bump = 'patch',
    [string]$Version,
    [string]$Configuration = 'Release',
    [string]$Source = 'https://api.nuget.org/v3/index.json',
    [string]$ApiKey = $env:NUGET_API_KEY,
    [switch]$SkipBuild,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'STS2-RitsuLib.csproj'
$manifestPath = Join-Path $repoRoot 'mod_manifest.json'
$constPath = Join-Path $repoRoot 'Const.cs'
$artifactsDir = Join-Path $repoRoot 'artifacts/nuget'

if (-not (Test-Path $projectPath)) {
    throw "Project file not found: $projectPath"
}

if (-not (Test-Path $manifestPath)) {
    throw "Manifest file not found: $manifestPath"
}

if (-not (Test-Path $constPath)) {
    throw "Const file not found: $constPath"
}

function Write-FileUtf8NoBom {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}

function Parse-Version {
    param([Parameter(Mandatory = $true)][string]$Value)

    $parsed = $null
    if (-not [System.Version]::TryParse($Value, [ref]$parsed)) {
        throw "Invalid version format: '$Value'. Use semantic format like 1.2.3"
    }

    return [System.Version]::Parse($Value)
}

function Get-ProjectVersion {
    param([Parameter(Mandatory = $true)][string]$Path)

    $content = Get-Content -Path $Path -Raw
    $match = [System.Text.RegularExpressions.Regex]::Match($content, '<Version>\s*([^<]+?)\s*</Version>')
    if (-not $match.Success) {
        throw "Failed to locate <Version> in $Path"
    }

    return $match.Groups[1].Value.Trim()
}

function Update-ProjectVersion {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewVersion
    )

    $content = Get-Content -Path $Path -Raw
    $pattern = '(<Version>\s*)([^<]+?)(\s*</Version>)'
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, $pattern)) {
        throw "Failed to locate <Version> in $Path"
    }

    $updated = [System.Text.RegularExpressions.Regex]::Replace(
        $content,
        $pattern,
        { param($m) $m.Groups[1].Value + $NewVersion + $m.Groups[3].Value },
        1
    )

    if ($updated -ne $content) {
        Write-FileUtf8NoBom -Path $Path -Content $updated
    }
}

function Update-ManifestVersion {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewVersion
    )

    $content = Get-Content -Path $Path -Raw
    $pattern = '"version"\s*:\s*"[^"]+"'
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, $pattern)) {
        throw "Failed to locate version field in $Path"
    }

    $replacement = '"version": "' + $NewVersion + '"'
    $updated = [System.Text.RegularExpressions.Regex]::Replace(
        $content,
        $pattern,
        $replacement,
        1
    )

    if ($updated -ne $content) {
        Write-FileUtf8NoBom -Path $Path -Content $updated
    }
}

function Update-ConstVersion {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewVersion
    )

    $content = Get-Content -Path $Path -Raw
    $pattern = 'public\s+const\s+string\s+Version\s*=\s*"[^"]+";'
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, $pattern)) {
        throw "Failed to locate Const.Version in $Path"
    }

    $replacement = 'public const string Version = "' + $NewVersion + '";'
    $updated = [System.Text.RegularExpressions.Regex]::Replace(
        $content,
        $pattern,
        $replacement,
        1
    )

    if ($updated -ne $content) {
        Write-FileUtf8NoBom -Path $Path -Content $updated
    }
}
$currentVersionText = Get-ProjectVersion -Path $projectPath

$currentVersion = Parse-Version -Value $currentVersionText

if ($Version) {
    $nextVersion = Parse-Version -Value $Version
}
else {
    switch ($Bump) {
        'major' { $nextVersion = [System.Version]::new($currentVersion.Major + 1, 0, 0) }
        'minor' { $nextVersion = [System.Version]::new($currentVersion.Major, $currentVersion.Minor + 1, 0) }
        'patch' { $nextVersion = [System.Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build + 1) }
        default { $nextVersion = $currentVersion }
    }
}

$nextVersionText = $nextVersion.ToString(3)
Update-ProjectVersion -Path $projectPath -NewVersion $nextVersionText
Update-ManifestVersion -Path $manifestPath -NewVersion $nextVersionText
Update-ConstVersion -Path $constPath -NewVersion $nextVersionText

Write-Host "Version synchronized: $currentVersionText -> $nextVersionText"
Write-Host " - STS2-RitsuLib.csproj"
Write-Host " - mod_manifest.json"
Write-Host " - Const.cs"

if (-not (Test-Path $artifactsDir)) {
    [void](New-Item -Path $artifactsDir -ItemType Directory -Force)
}

$packArgs = @('pack', $projectPath, '-c', $Configuration, '-o', $artifactsDir, '/p:ContinuousIntegrationBuild=false')
if ($SkipBuild) {
    $packArgs += '--no-build'
}

Write-Host "Packing project..."
& dotnet @packArgs
if ($LASTEXITCODE -ne 0) {
    throw 'dotnet pack failed.'
}

$packageFile = Get-ChildItem -Path $artifactsDir -Filter '*.nupkg' |
    Where-Object { $_.Name -notlike '*.snupkg' } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $packageFile) {
    throw "No .nupkg generated under $artifactsDir"
}

if ($DryRun) {
    Write-Host "[DryRun] Package generated: $($packageFile.FullName)"
    Write-Host "[DryRun] Skip push. Target source: $Source"
    exit 0
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw 'NuGet API key missing. Pass -ApiKey or set NUGET_API_KEY.'
}

Write-Host 'Pushing package to NuGet source...'
& dotnet nuget push $packageFile.FullName --source $Source --api-key $ApiKey --skip-duplicate
if ($LASTEXITCODE -ne 0) {
    throw 'dotnet nuget push failed.'
}

Write-Host "Publish complete: $($packageFile.Name)"
