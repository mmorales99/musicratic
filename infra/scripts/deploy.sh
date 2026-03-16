#!/bin/bash
set -euo pipefail

# Musicratic production deployment script
# Usage: ./deploy.sh [--pull-only]
#
# Expects:
#   - Podman and podman-compose installed on the VPS
#   - Secrets files present in infra/secrets/
#   - MUSICRATIC_DEPLOY_DIR env var (defaults to /opt/musicratic)

DEPLOY_DIR="${MUSICRATIC_DEPLOY_DIR:-/opt/musicratic}"
COMPOSE_FILE="$DEPLOY_DIR/infra/podman-compose.yml"

echo "=== Musicratic Deployment ==="
echo "Deploy directory: $DEPLOY_DIR"
echo "Timestamp: $(date -u +%Y-%m-%dT%H:%M:%SZ)"

# Pull latest code
cd "$DEPLOY_DIR"
git pull origin main

# Build and restart services
echo "=== Building and starting services ==="
podman-compose -f "$COMPOSE_FILE" build --no-cache
podman-compose -f "$COMPOSE_FILE" up -d

# Wait for health checks
echo "=== Waiting for services to become healthy ==="
sleep 15

# Verify critical services
SERVICES=("musicratic-backend" "musicratic-bff-web" "musicratic-bff-mobile" "musicratic-postgres" "musicratic-caddy")
for svc in "${SERVICES[@]}"; do
    STATUS=$(podman inspect --format='{{.State.Health.Status}}' "$svc" 2>/dev/null || echo "unknown")
    echo "  $svc: $STATUS"
    if [ "$STATUS" != "healthy" ]; then
        echo "  WARNING: $svc is not healthy yet"
    fi
done

echo "=== Deployment complete ==="
