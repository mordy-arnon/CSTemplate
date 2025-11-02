#!/usr/bin/env pwsh
# Docker Build Script
# Usage: ./scripts/docker-build.ps1 [-Tag <tag>] [-Push] [-Registry <registry>]

param(
    [string]$Tag = "latest",
    [string]$ImageName = "cstemplate-api",
    [string]$Registry = "",
    [switch]$Push,
    [switch]$NoCache
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Docker Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Construct full image name
if ($Registry) {
    $FullImageName = "$Registry/$ImageName`:$Tag"
} else {
    $FullImageName = "$ImageName`:$Tag"
}

Write-Host "Image: $FullImageName" -ForegroundColor Cyan
Write-Host ""

# Build Docker image
Write-Host "[1/3] Building Docker image..." -ForegroundColor Yellow
$buildArgs = @(
    "build",
    "-t", $FullImageName,
    "-f", "Dockerfile"
)

if ($NoCache) {
    $buildArgs += "--no-cache"
}

$buildArgs += "."

& docker @buildArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Docker build failed" -ForegroundColor Red
    exit 1
}

# Show image info
Write-Host ""
Write-Host "[2/3] Image built successfully:" -ForegroundColor Green
docker images $FullImageName

# Push if requested
if ($Push) {
    Write-Host ""
    Write-Host "[3/3] Pushing image to registry..." -ForegroundColor Yellow
    docker push $FullImageName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "  ✓ Image built and pushed!" -ForegroundColor Green
        Write-Host "  $FullImageName" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
    } else {
        Write-Host "✗ Docker push failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "  ✓ Image built successfully!" -ForegroundColor Green
    Write-Host "  $FullImageName" -ForegroundColor Green
    Write-Host ""
    Write-Host "  To push: ./scripts/docker-build.ps1 -Push" -ForegroundColor Yellow
    Write-Host "  To run:  docker run -p 5000:5000 $FullImageName" -ForegroundColor Yellow
    Write-Host "=====================================" -ForegroundColor Green
}

