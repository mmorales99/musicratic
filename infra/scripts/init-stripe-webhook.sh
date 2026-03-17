#!/bin/bash
# =============================================================================
# Stripe Webhook Forwarder — Local Development
# Requires: Stripe CLI installed and authenticated (stripe login)
# =============================================================================

set -euo pipefail

FORWARD_URL="${STRIPE_FORWARD_URL:-http://localhost:5000/api/economy/webhooks/stripe}"

echo "Starting Stripe webhook forwarder..."
echo "Forwarding events to: ${FORWARD_URL}"
echo ""
echo "Press Ctrl+C to stop."
echo ""

stripe listen --forward-to "${FORWARD_URL}"
