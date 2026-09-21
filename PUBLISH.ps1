# run 'Set-ExecutionPolicy -ExecutionPolicy RemoteSigned' 
# if PowerShell script execution is disabled on your system

#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$TargetFramework = 'net8.0-windows10.0.19041.0',
    [string]$RuntimeIdentifier = 'win10-x64',
    [string]$OutputDir,
    [string[]]$KeepLanguages = @('en', 'en-US')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# --- 1. Locate the .csproj ---
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$csproj = Get-ChildItem -Path $scriptDir -Filter *.csproj -File | Select-Object -First 1

if (-not $csproj) {
    Write-Error "[ERROR] No .csproj file found in: $scriptDir"
    exit 1
}

# --- 2. Output folder ---
if (-not $OutputDir) {
    $OutputDir = Join-Path $scriptDir 'publish_output'
}

Write-Host "Found project : $($csproj.FullName)"
Write-Host "Output folder : $OutputDir"
Write-Host "Starting build..."
Write-Host

# --- 3. Publish ---
$publishArgs = @(
    'publish', $csproj.FullName,
    '-f', $TargetFramework,
    '-c', $Configuration,
    "-p:RuntimeIdentifierOverride=$RuntimeIdentifier",
    '-p:WindowsPackageType=None',
    '--output', $OutputDir
)

& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "[ERROR] dotnet publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host
Write-Host "Build finished. Starting language cleanup..."
Write-Host

# --- 4. Sanity check ---
if (-not (Test-Path -LiteralPath $OutputDir -PathType Container)) {
    Write-Error "[ERROR] Publish directory not found: $OutputDir"
    exit 1
}

# --- 5. Remove language folders ---
# Matches: xx, xx-XX, xxx-XX, xx-XXXX, etc.
# (2 or 3 letter language code, optionally followed by hyphen + region/script)
$langPattern = '^[a-z]{2,3}(-[A-Za-z0-9]+)*$'

$removed = 0
Get-ChildItem -LiteralPath $OutputDir -Directory | ForEach-Object {
    $name = $_.Name

    if ($name -match $langPattern -and $KeepLanguages -notcontains $name) {
        Write-Host "Removing language folder: $name"
        Remove-Item -LiteralPath $_.FullName -Recurse -Force
        $removed++
    }
}

Write-Host
Write-Host "[SUCCESS] Cleanup finished ($removed folder(s) removed)."
Write-Host "Final binaries are at: $OutputDir"