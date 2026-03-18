#!/usr/bin/env bash
# Generate merged coverage report (backend + web + mobile)
# Requires: dotnet-reportgenerator-globaltool
#   dotnet tool install -g dotnet-reportgenerator-globaltool

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
COVERAGE_DIR="$ROOT_DIR/coverage"

echo "=== Musicratic Coverage Report Generator ==="
echo ""

# Collect backend coverage
echo "[1/4] Running backend tests with coverage..."
dotnet test "$ROOT_DIR/src/Musicratic.slnx" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$COVERAGE_DIR/backend" \
  --logger "console;verbosity=minimal" \
  2>/dev/null || true

# Collect web coverage (optional — skip if web not set up)
if [ -f "$ROOT_DIR/web/package.json" ]; then
  echo "[2/4] Running web tests with coverage..."
  (cd "$ROOT_DIR/web" && npx ng test --watch=false --code-coverage 2>/dev/null) || true
  mkdir -p "$COVERAGE_DIR/web"
  [ -f "$ROOT_DIR/web/coverage/lcov.info" ] && \
    cp "$ROOT_DIR/web/coverage/lcov.info" "$COVERAGE_DIR/web/"
else
  echo "[2/4] Skipping web (not found)"
fi

# Collect mobile coverage (optional — skip if mobile not set up)
if [ -f "$ROOT_DIR/mobile/pubspec.yaml" ]; then
  echo "[3/4] Running mobile tests with coverage..."
  (cd "$ROOT_DIR/mobile" && flutter test --coverage 2>/dev/null) || true
  mkdir -p "$COVERAGE_DIR/mobile"
  [ -f "$ROOT_DIR/mobile/coverage/lcov.info" ] && \
    cp "$ROOT_DIR/mobile/coverage/lcov.info" "$COVERAGE_DIR/mobile/"
else
  echo "[3/4] Skipping mobile (not found)"
fi

# Build report inputs
REPORTS=""
[ -n "$(find "$COVERAGE_DIR/backend" -name 'coverage.cobertura.xml' 2>/dev/null)" ] && \
  REPORTS="$COVERAGE_DIR/backend/**/coverage.cobertura.xml"
[ -f "$COVERAGE_DIR/web/lcov.info" ] && \
  REPORTS="${REPORTS:+$REPORTS;}$COVERAGE_DIR/web/lcov.info"
[ -f "$COVERAGE_DIR/mobile/lcov.info" ] && \
  REPORTS="${REPORTS:+$REPORTS;}$COVERAGE_DIR/mobile/lcov.info"

if [ -z "$REPORTS" ]; then
  echo "No coverage data found. Exiting."
  exit 1
fi

# Generate merged report
echo "[4/4] Generating merged HTML report..."
reportgenerator \
  "-reports:$REPORTS" \
  "-targetdir:$COVERAGE_DIR/report" \
  "-reporttypes:Html;Cobertura;TextSummary" \
  "-title:Musicratic Coverage Report"

echo ""
echo "=== Coverage Report ==="
cat "$COVERAGE_DIR/report/Summary.txt" 2>/dev/null || echo "(summary not available)"
echo ""
echo "HTML report: $COVERAGE_DIR/report/index.html"
