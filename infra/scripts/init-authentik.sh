#!/bin/bash
set -euo pipefail

# ── Authentik OIDC Provider Setup ──────────────────────────────────────
# Reference script: configures Authentik via its API to create an
# OAuth2/OIDC provider and application for Musicratic.
#
# Prerequisites:
#   - Authentik must be running and healthy
#   - An admin account must exist (created on first Authentik startup)
#   - Set AUTHENTIK_URL and AUTHENTIK_TOKEN before running
#
# Usage:
#   export AUTHENTIK_URL="http://localhost:9000"
#   export AUTHENTIK_TOKEN="<your-admin-api-token>"
#   bash infra/scripts/init-authentik.sh

AUTHENTIK_URL="${AUTHENTIK_URL:-http://localhost:9000}"
AUTHENTIK_TOKEN="${AUTHENTIK_TOKEN:?Error: AUTHENTIK_TOKEN must be set (generate from Authentik Admin > Directory > Tokens)}"
API="${AUTHENTIK_URL}/api/v3"

PROVIDER_NAME="musicratic-web"
APPLICATION_NAME="musicratic-web"
APPLICATION_SLUG="musicratic-web"
REDIRECT_URI="https://localhost:3000/callback"
SCOPES="openid email profile"

echo "==> Configuring Authentik at ${AUTHENTIK_URL}"

# ── 1. Fetch built-in scope mappings (openid, email, profile) ─────────
echo "  [1/4] Fetching scope mappings..."
SCOPE_MAPPINGS_JSON=$(curl -sf \
    -H "Authorization: Bearer ${AUTHENTIK_TOKEN}" \
    "${API}/propertymappings/scope/?ordering=scope_name&page_size=50")

SCOPE_IDS=""
for SCOPE in $SCOPES; do
    SCOPE_ID=$(echo "$SCOPE_MAPPINGS_JSON" \
        | python3 -c "
import sys, json
data = json.load(sys.stdin)
for r in data['results']:
    if r.get('scope_name') == '${SCOPE}':
        print(r['pk'])
        break
")
    if [ -z "$SCOPE_ID" ]; then
        echo "  ERROR: Scope '${SCOPE}' not found in Authentik."
        exit 1
    fi
    SCOPE_IDS="${SCOPE_IDS}\"${SCOPE_ID}\","
done
# Remove trailing comma
SCOPE_IDS="[${SCOPE_IDS%,}]"
echo "    Scope IDs: ${SCOPE_IDS}"

# ── 2. Fetch authorization and invalidation flow slugs ────────────────
echo "  [2/4] Fetching flows..."
AUTH_FLOW_SLUG=$(curl -sf \
    -H "Authorization: Bearer ${AUTHENTIK_TOKEN}" \
    "${API}/flows/instances/?designation=authorization&ordering=slug" \
    | python3 -c "
import sys, json
data = json.load(sys.stdin)
if data['results']:
    print(data['results'][0]['slug'])
")

INVALIDATION_FLOW_SLUG=$(curl -sf \
    -H "Authorization: Bearer ${AUTHENTIK_TOKEN}" \
    "${API}/flows/instances/?designation=invalidation&ordering=slug" \
    | python3 -c "
import sys, json
data = json.load(sys.stdin)
if data['results']:
    print(data['results'][0]['slug'])
")

echo "    Authorization flow: ${AUTH_FLOW_SLUG}"
echo "    Invalidation flow:  ${INVALIDATION_FLOW_SLUG}"

# ── 3. Create OAuth2/OIDC Provider ───────────────────────────────────
echo "  [3/4] Creating OAuth2/OIDC provider '${PROVIDER_NAME}'..."
PROVIDER_RESPONSE=$(curl -sf \
    -X POST \
    -H "Authorization: Bearer ${AUTHENTIK_TOKEN}" \
    -H "Content-Type: application/json" \
    "${API}/providers/oauth2/" \
    -d "{
        \"name\": \"${PROVIDER_NAME}\",
        \"authorization_flow\": \"${AUTH_FLOW_SLUG}\",
        \"invalidation_flow\": \"${INVALIDATION_FLOW_SLUG}\",
        \"property_mappings\": ${SCOPE_IDS},
        \"client_type\": \"confidential\",
        \"redirect_uris\": \"${REDIRECT_URI}\",
        \"signing_key\": null,
        \"sub_mode\": \"hashed_user_id\",
        \"include_claims_in_id_token\": true,
        \"access_token_validity\": \"minutes=10\",
        \"refresh_token_validity\": \"days=30\"
    }")

PROVIDER_PK=$(echo "$PROVIDER_RESPONSE" | python3 -c "import sys,json; print(json.load(sys.stdin)['pk'])")
CLIENT_ID=$(echo "$PROVIDER_RESPONSE" | python3 -c "import sys,json; print(json.load(sys.stdin)['client_id'])")
CLIENT_SECRET=$(echo "$PROVIDER_RESPONSE" | python3 -c "import sys,json; print(json.load(sys.stdin)['client_secret'])")

echo "    Provider PK:    ${PROVIDER_PK}"
echo "    Client ID:      ${CLIENT_ID}"
echo "    Client Secret:  ${CLIENT_SECRET}"

# ── 4. Create Application ────────────────────────────────────────────
echo "  [4/4] Creating application '${APPLICATION_NAME}'..."
curl -sf \
    -X POST \
    -H "Authorization: Bearer ${AUTHENTIK_TOKEN}" \
    -H "Content-Type: application/json" \
    "${API}/core/applications/" \
    -d "{
        \"name\": \"${APPLICATION_NAME}\",
        \"slug\": \"${APPLICATION_SLUG}\",
        \"provider\": ${PROVIDER_PK},
        \"meta_launch_url\": \"https://localhost:3000\",
        \"policy_engine_mode\": \"any\"
    }" > /dev/null

echo ""
echo "==> Authentik OIDC configuration complete!"
echo ""
echo "  Application:   ${APPLICATION_NAME}"
echo "  OIDC Issuer:   ${AUTHENTIK_URL}/application/o/${APPLICATION_SLUG}/"
echo "  Client ID:     ${CLIENT_ID}"
echo "  Client Secret: ${CLIENT_SECRET}"
echo "  Redirect URI:  ${REDIRECT_URI}"
echo "  Scopes:        ${SCOPES}"
echo ""
echo "  Save Client ID and Client Secret in infra/config/authentik-oidc.json"
echo "  or as environment variables MUSICRATIC_OIDC_CLIENT_ID / MUSICRATIC_OIDC_CLIENT_SECRET."
