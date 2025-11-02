#!/usr/bin/env pwsh
# Run Linter (Code Analysis)
# Usage: ./scripts/lint.ps1

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Running Linter" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Run dotnet build with warnings as errors
Write-Host "[1/2] Running code analysis..." -ForegroundColor Yellow
try {
    dotnet build `
        --configuration Release `
        /p:TreatWarningsAsErrors=true `
        /p:AnalysisLevel=latest `
        /p:EnforceCodeStyleInBuild=true `
        /p:EnableNETAnalyzers=true
    
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "  ✓ Linting PASSED - No issues found" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host "  ✗ Linting FAILED - Issues found" -ForegroundColor Red
    Write-Host "=====================================" -ForegroundColor Red
    exit 1
}

