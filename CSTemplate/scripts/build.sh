#!/bin/bash
# Build and Package Script
# Usage: ./scripts/build.sh

set -e

CONFIGURATION="${1:-Release}"
OUTPUT_PATH="${2:-./publish}"

echo "====================================="
echo "  Building CSTemplate"
echo "====================================="
echo ""

# Clean previous build
echo "[1/4] Cleaning previous build..."
if [ -d "$OUTPUT_PATH" ]; then
    rm -rf "$OUTPUT_PATH"
fi
dotnet clean --configuration "$CONFIGURATION"

# Restore dependencies
echo "[2/4] Restoring dependencies..."
dotnet restore

# Build
echo "[3/4] Building project..."
dotnet build --configuration "$CONFIGURATION" --no-restore

# Publish
echo "[4/4] Publishing application..."
dotnet publish --configuration "$CONFIGURATION" --output "$OUTPUT_PATH" --no-build

echo ""
echo "====================================="
echo "  Build completed successfully!"
echo "  Output: $OUTPUT_PATH"
echo "====================================="

