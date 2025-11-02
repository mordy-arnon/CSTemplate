#!/usr/bin/env pwsh
# Run Tests with Coverage Check
# Usage: ./scripts/test.ps1

param(
    [int]$MinCoverage = 80,
    [string]$ReportPath = "./coverage"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Running Tests with Coverage" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Check if coverlet is installed
Write-Host "[1/4] Checking coverage tools..." -ForegroundColor Yellow
$hasReportGenerator = dotnet tool list --global | Select-String "dotnet-reportgenerator-globaltool"
if (-not $hasReportGenerator) {
    Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

# Clean previous coverage
Write-Host "[2/4] Cleaning previous coverage..." -ForegroundColor Yellow
if (Test-Path $ReportPath) {
    Remove-Item -Path $ReportPath -Recurse -Force
}
New-Item -ItemType Directory -Path $ReportPath -Force | Out-Null

# Run tests with coverage
Write-Host "[3/4] Running tests with coverage..." -ForegroundColor Yellow
dotnet test `
    --configuration Release `
    --collect:"XPlat Code Coverage" `
    --results-directory "$ReportPath/raw" `
    --logger "console;verbosity=normal" `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput="$ReportPath/"

# Generate HTML report
Write-Host "[4/4] Generating coverage report..." -ForegroundColor Yellow
$coverageFile = Get-ChildItem -Path "$ReportPath/raw" -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1

if ($coverageFile) {
    reportgenerator `
        -reports:"$($coverageFile.FullName)" `
        -targetdir:"$ReportPath/html" `
        -reporttypes:"Html;TextSummary"
    
    # Read and display coverage summary
    $summaryFile = "$ReportPath/html/Summary.txt"
    if (Test-Path $summaryFile) {
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Cyan
        Write-Host "  Coverage Summary" -ForegroundColor Cyan
        Write-Host "=====================================" -ForegroundColor Cyan
        Get-Content $summaryFile
        
        # Extract line coverage percentage
        $summary = Get-Content $summaryFile -Raw
        if ($summary -match "Line coverage: (\d+\.?\d*)%") {
            $coverage = [double]$matches[1]
            
            Write-Host ""
            if ($coverage -ge $MinCoverage) {
                Write-Host "✓ Coverage check PASSED: $coverage% >= $MinCoverage%" -ForegroundColor Green
                Write-Host "  HTML Report: $ReportPath/html/index.html" -ForegroundColor Green
                exit 0
            } else {
                Write-Host "✗ Coverage check FAILED: $coverage% < $MinCoverage%" -ForegroundColor Red
                Write-Host "  HTML Report: $ReportPath/html/index.html" -ForegroundColor Yellow
                exit 1
            }
        }
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "  Tests completed!" -ForegroundColor Green
Write-Host "  HTML Report: $ReportPath/html/index.html" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

