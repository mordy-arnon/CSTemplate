#!/bin/bash
# Run Tests with Coverage Check
# Usage: ./scripts/test.sh [min-coverage]

set -e

MIN_COVERAGE="${1:-80}"
REPORT_PATH="./coverage"

echo "====================================="
echo "  Running Tests with Coverage"
echo "====================================="
echo ""

# Check if reportgenerator is installed
echo "[1/4] Checking coverage tools..."
if ! dotnet tool list --global | grep -q "dotnet-reportgenerator-globaltool"; then
    echo "Installing ReportGenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
fi

# Clean previous coverage
echo "[2/4] Cleaning previous coverage..."
rm -rf "$REPORT_PATH"
mkdir -p "$REPORT_PATH"

# Run tests with coverage
echo "[3/4] Running tests with coverage..."
dotnet test \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory "$REPORT_PATH/raw" \
  --logger "console;verbosity=normal"


# Generate HTML report
echo "[4/4] Generating coverage report..."
COVERAGE_FILE=$(find "$REPORT_PATH/raw" -name "coverage.cobertura.xml" | head -n 1)

if [ -f "$COVERAGE_FILE" ]; then
    reportgenerator \
        -reports:"$COVERAGE_FILE" \
        -targetdir:"$REPORT_PATH/html" \
        -reporttypes:"Html;TextSummary"
    
    # Display summary
    if [ -f "$REPORT_PATH/html/Summary.txt" ]; then
        echo ""
        echo "====================================="
        echo "  Coverage Summary"
        echo "====================================="
        cat "$REPORT_PATH/html/Summary.txt"
        
        # Extract coverage percentage
        COVERAGE=$(grep "Line coverage:" "$REPORT_PATH/html/Summary.txt" | grep -oP '\d+\.?\d*(?=%)')
        
        echo ""
        if awk "BEGIN {exit !($COVERAGE >= $MIN_COVERAGE)}"; then
            echo "✓ Coverage check PASSED: $COVERAGE% >= $MIN_COVERAGE%"
            echo "  HTML Report: $REPORT_PATH/html/index.html"
            exit 0
        else
            echo "✗ Coverage check FAILED: $COVERAGE% < $MIN_COVERAGE%"
            echo "  HTML Report: $REPORT_PATH/html/index.html"
            exit 1
        fi

    fi
fi

echo ""
echo "====================================="
echo "  Tests completed!"
echo "  HTML Report: $REPORT_PATH/html/index.html"
echo "====================================="

