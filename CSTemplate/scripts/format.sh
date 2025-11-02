#!/bin/bash
# Run Code Formatter
# Usage: ./scripts/format.sh [--check|--fix]

set -e

MODE="${1:check}"

echo "====================================="
echo "  Running Code Formatter"
echo "====================================="
echo ""

if [ "$MODE" == "--fix" ]; then
    echo "[1/1] Formatting code..."
    dotnet format CSTemplate.sln
    
    echo ""
    echo "====================================="
    echo "  ✓ Code formatted successfully"
    echo "====================================="
else
    echo "[1/1] Checking code format..."
    if dotnet format CSTemplate.sln --verify-no-changes; then
        echo ""
        echo "====================================="
        echo "  ✓ Format check PASSED"
        echo "====================================="
        exit 0
    else
        echo ""
        echo "====================================="
        echo "  ✗ Format check FAILED"
        echo "  Run './scripts/format.sh --fix' to fix"
        echo "====================================="
        exit 1
    fi
fi

