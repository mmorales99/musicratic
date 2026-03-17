#!/bin/bash
# quality-gate-check.sh — Polls SonarQube API for quality gate status
# Usage: ./quality-gate-check.sh <project-key> <sonar-host-url> <sonar-token>

set -euo pipefail

PROJECT_KEY="${1:?Usage: $0 <project-key> <sonar-host-url> <sonar-token>}"
SONAR_HOST="${2:?Missing sonar host URL}"
SONAR_TOKEN="${3:?Missing sonar token}"
MAX_ATTEMPTS=30
POLL_INTERVAL=10

echo "Checking quality gate for project: ${PROJECT_KEY}"
echo "SonarQube host: ${SONAR_HOST}"

for attempt in $(seq 1 "${MAX_ATTEMPTS}"); do
  echo "Polling analysis status (attempt ${attempt}/${MAX_ATTEMPTS})..."

  RESPONSE=$(curl -s -u "${SONAR_TOKEN}:" \
    "${SONAR_HOST}/api/qualitygates/project_status?projectKey=${PROJECT_KEY}")

  STATUS=$(echo "${RESPONSE}" | python3 -c "import sys,json; print(json.load(sys.stdin)['projectStatus']['status'])" 2>/dev/null || echo "PENDING")

  case "${STATUS}" in
    OK)
      echo "Quality gate PASSED"
      exit 0
      ;;
    ERROR)
      echo "Quality gate FAILED"
      echo "${RESPONSE}" | python3 -m json.tool 2>/dev/null || echo "${RESPONSE}"
      exit 1
      ;;
    WARN)
      echo "Quality gate WARNING — treating as pass"
      exit 0
      ;;
    *)
      echo "Analysis not ready (status: ${STATUS}), waiting ${POLL_INTERVAL}s..."
      sleep "${POLL_INTERVAL}"
      ;;
  esac
done

echo "Timeout: quality gate status not available after $((MAX_ATTEMPTS * POLL_INTERVAL))s"
exit 1
