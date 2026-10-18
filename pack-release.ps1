# Build, pack a .pext with Playnite Toolbox, and optionally publish a GitHub release.
# Usage:
#   .\pack-release.ps1
#   .\pack-release.ps1 -Publish
#   .\pack-release.ps1 -ToolboxPath "C:\Path\To\Toolbox.exe" -Publish

[CmdletBinding()]
param(
    [string]$ToolboxPath = "",
    [switch]$Publish,
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

function Resolve-ToolboxPath([string]$explicit) {
    if (-not [string]::IsNullOrWhiteSpace($explicit) -and (Test-Path -LiteralPath $explicit)) {
        return (Resolve-Path -LiteralPath $explicit).Path
    }

    if (-not [string]::IsNullOrWhiteSpace($env:PLAYNITE_TOOLBOX) -and (Test-Path -LiteralPath $env:PLAYNITE_TOOLBOX)) {
        return (Resolve-Path -LiteralPath $env:PLAYNITE_TOOLBOX).Path
    }

    $candidates = @(
        (Join-Path $env:LOCALAPPDATA "Playnite\Toolbox.exe"),
        (Join-Path ${env:ProgramFiles} "Playnite\Toolbox.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Playnite\Toolbox.exe")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    throw "Toolbox.exe not found. Install Playnite or pass -ToolboxPath / set PLAYNITE_TOOLBOX."
}

$extensionYaml = Join-Path $root "src\PrismLibrary\extension.yaml"
if (-not (Test-Path -LiteralPath $extensionYaml)) {
    throw "Missing extension.yaml at $extensionYaml"
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = (Select-String -Path $extensionYaml -Pattern '^\s*Version:\s*(.+)\s*$').Matches[0].Groups[1].Value.Trim()
}

if ($Version -notmatch '^\d+(\.\d+){1,3}$') {
    throw "Invalid version '$Version'."
}

Write-Host "Running build-and-test.ps1..."
& (Join-Path $root "build-and-test.ps1")
if ($LASTEXITCODE -ne 0) {
    throw "build-and-test.ps1 failed."
}

$pluginOut = Join-Path $root "src\PrismLibrary\bin\Release\net462"
if (-not (Test-Path -LiteralPath (Join-Path $pluginOut "PrismLibrary.dll"))) {
    throw "Built plugin not found under $pluginOut"
}

$artifacts = Join-Path $root "artifacts"
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

$versionFile = $Version.Replace('.', '_')
$packageName = "PrismLibrary_$versionFile.pext"
$toolbox = Resolve-ToolboxPath $ToolboxPath

Write-Host "Packing with Toolbox: $toolbox"
& $toolbox pack $pluginOut $artifacts
if ($LASTEXITCODE -ne 0) {
    throw "Toolbox pack failed."
}

# Toolbox names the package from extension Name ("Prism Library.pext"); normalize.
$produced = Get-ChildItem -LiteralPath $artifacts -Filter "*.pext" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if ($null -eq $produced) {
    throw "No .pext produced in $artifacts"
}

$target = Join-Path $artifacts $packageName
if ($produced.FullName -ne $target) {
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Force
    }
    Move-Item -LiteralPath $produced.FullName -Destination $target -Force
}

Write-Host "Package: $target"

$tag = "v$Version"
$packageUrl = "https://github.com/Loneranger419/playnite-prism-library/releases/download/$tag/$packageName"
Write-Host "Expected PackageUrl: $packageUrl"

if ($Publish) {
    $notes = @"
Prism Library $Version

See installer.yaml / README for install and features.
"@
    Write-Host "Creating GitHub release $tag ..."
    gh release create $tag $target `
        --title "Prism Library $Version" `
        --notes $notes `
        --repo Loneranger419/playnite-prism-library
    if ($LASTEXITCODE -ne 0) {
        throw "gh release create failed."
    }

    Write-Host "Verifying installer manifest..."
    & $toolbox verify installer (Join-Path $root "installer.yaml")
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Installer verify reported issues. Confirm PackageUrl matches the uploaded asset name."
    }
}

Write-Host "Done."
