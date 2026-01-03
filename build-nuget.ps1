#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build NuGet package for blazor-net-app
.DESCRIPTION
    This script builds a NuGet package for the BlazorNetApp.Api project
.PARAMETER Version
    The version number for the package (e.g., 1.0.0)
.EXAMPLE
    .\build-nuget.ps1 -Version 1.0.0
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "Building NuGet package for version: $Version" -ForegroundColor Green

# Verify dotnet is installed
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "Error: dotnet command not found. Please install .NET SDK."
    exit 1
}

# Create build directory
$buildDir = "build"
if (-not (Test-Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir | Out-Null
}

# Build the NuGet package
Write-Host "Building NuGet package..." -ForegroundColor Yellow
try {
    dotnet pack src/BlazorNetApp.Api/BlazorNetApp.Api.csproj `
        --configuration Release `
        --output $buildDir `
        -p:PackageVersion=$Version `
        -p:Version=$Version
    
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Error "Failed to build NuGet package: $_"
    exit 1
}

$packageName = "BlazorNetApp.$Version.nupkg"
$packagePath = Join-Path $buildDir $packageName

if (Test-Path $packagePath) {
    Write-Host "✓ NuGet package created: $packagePath" -ForegroundColor Green
    Write-Host "✓ Build complete!" -ForegroundColor Green
} else {
    Write-Error "Package file not found at expected path: $packagePath"
    exit 1
}
