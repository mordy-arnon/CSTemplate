#!/usr/bin/env pwsh
# Run Code Formatter
# Usage: ./scripts/format.ps1 [-Check] [-Fix]

param(
    [switch]$Check,
    [switch]$Fix
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Running Code Formatter" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

if ($Fix -or -not $Check) {
    Write-Host "[1/1] Formatting code..." -ForegroundColor Yellow
    dotnet format --verbosity diagnostic
    
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "  ✓ Code formatted successfully" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
}
elseif ($Check) {
    Write-Host "[1/1] Checking code format..." -ForegroundColor Yellow
    
    try {
        dotnet format --verify-no-changes --verbosity diagnostic
        
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "  ✓ Format check PASSED" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        exit 0
    }
    catch {
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Red
        Write-Host "  ✗ Format check FAILED" -ForegroundColor Red
        Write-Host "  Run './scripts/format.ps1 -Fix' to fix" -ForegroundColor Yellow
        Write-Host "=====================================" -ForegroundColor Red
        exit 1
    }
}

