#!/usr/bin/env pwsh
# Build and Package Script
# Usage: ./scripts/build.ps1

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "./publish"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Building CSTemplate" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Set error action
$ErrorActionPreference = "Stop"

# Clean previous build
Write-Host "[1/4] Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
}
dotnet clean --configuration $Configuration

# Restore dependencies
Write-Host "[2/4] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build
Write-Host "[3/4] Building project..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

# Publish
Write-Host "[4/4] Publishing application..." -ForegroundColor Yellow
dotnet publish --configuration $Configuration --output $OutputPath --no-build

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "  Build completed successfully!" -ForegroundColor Green
Write-Host "  Output: $OutputPath" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

