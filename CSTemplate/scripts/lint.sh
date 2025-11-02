#!/bin/bash
# Run Linter (Code Analysis)
# Usage: ./scripts/lint.sh

set -e

echo "====================================="
echo "  Running Linter"
echo "====================================="
echo ""

# Run dotnet build with warnings as errors
echo "[1/2] Running code analysis..."
if dotnet build \
    --configuration Release \
    /p:TreatWarningsAsErrors=true \
    /p:AnalysisLevel=latest \
    /p:EnforceCodeStyleInBuild=true \
    /p:EnableNETAnalyzers=true; then
    
    echo ""
    echo "====================================="
    echo "  ✓ Linting PASSED - No issues found"
    echo "====================================="
    exit 0
else
    echo ""
    echo "====================================="
    echo "  ✗ Linting FAILED - Issues found"
    echo "====================================="
    exit 1
fi

