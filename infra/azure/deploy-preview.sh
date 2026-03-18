#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────
# Musicratic — Azure Preview Deployment
# Deploys: Container Apps (API) + PostgreSQL Flexible + Static Web Apps (SPA)
#
# Prerequisites:
#   - Azure CLI (az) logged in: az login
#   - Node.js + npm (for Angular build)
#   (No Docker/Podman needed — image is built in Azure via ACR Build Tasks)
#
# Usage:
#   chmod +x infra/azure/deploy-preview.sh
#   ./infra/azure/deploy-preview.sh
#
# Cost: ~$0/month on free tiers + ~$12/month for PostgreSQL Burstable B1ms
#        (covered by Azure Free Trial $200 credit)
# ──────────────────────────────────────────────────────────────
set -euo pipefail

# ── Configuration ───────────────────────────────────────────
RESOURCE_GROUP="${MUSICRATIC_RG:-musicratic-preview-rg}"
LOCATION="${MUSICRATIC_LOCATION:-westeurope}"
PG_SERVER_NAME="${MUSICRATIC_PG_SERVER:-musicratic-pg-preview}"
PG_ADMIN_USER="${MUSICRATIC_PG_USER:-musicratic_admin}"
PG_DB_NAME="${MUSICRATIC_PG_DB:-musicratic}"
ACR_NAME="${MUSICRATIC_ACR:-musicraticpreview}"
CONTAINER_APP_ENV="${MUSICRATIC_CAE:-musicratic-preview-env}"
CONTAINER_APP_NAME="${MUSICRATIC_CA:-musicratic-api}"
STATIC_WEB_APP_NAME="${MUSICRATIC_SWA:-musicratic-web-preview}"
IMAGE_TAG="latest"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "╔══════════════════════════════════════════════╗"
echo "║  Musicratic — Azure Preview Deployment       ║"
echo "╚══════════════════════════════════════════════╝"
echo ""
echo "Resource Group:  $RESOURCE_GROUP"
echo "Location:        $LOCATION"
echo "PostgreSQL:      $PG_SERVER_NAME"
echo "Container App:   $CONTAINER_APP_NAME"
echo "Static Web App:  $STATIC_WEB_APP_NAME"
echo ""

# ── Step 1: Resource Group ──────────────────────────────────
echo "▸ [1/8] Creating resource group..."
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output none

# ── Step 2: PostgreSQL Flexible Server ──────────────────────
echo "▸ [2/8] Creating PostgreSQL Flexible Server (Burstable B1ms)..."

# Generate a random password for PG
PG_PASSWORD="$(openssl rand -base64 32 | tr -d '/+=' | head -c 32)Aa1!"

az postgres flexible-server create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$PG_SERVER_NAME" \
  --location "$LOCATION" \
  --admin-user "$PG_ADMIN_USER" \
  --admin-password "$PG_PASSWORD" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 16 \
  --yes \
  --output none

# Allow Azure services to connect
az postgres flexible-server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$PG_SERVER_NAME" \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none

# Create database
az postgres flexible-server db create \
  --resource-group "$RESOURCE_GROUP" \
  --server-name "$PG_SERVER_NAME" \
  --database-name "$PG_DB_NAME" \
  --output none

# Create schemas
PG_HOST="${PG_SERVER_NAME}.postgres.database.azure.com"
PG_CONN="host=${PG_HOST} port=5432 dbname=${PG_DB_NAME} user=${PG_ADMIN_USER} password=${PG_PASSWORD} sslmode=require"

echo "▸ Creating database schemas..."
PGPASSWORD="$PG_PASSWORD" psql \
  -h "$PG_HOST" \
  -U "$PG_ADMIN_USER" \
  -d "$PG_DB_NAME" \
  -f "$REPO_ROOT/infra/scripts/init-schemas.sql" 2>/dev/null || \
  echo "  (psql not available — schemas will be created by EF migrations)"

# Build connection string
PG_CONN_STRING="Host=${PG_HOST};Port=5432;Database=${PG_DB_NAME};Username=${PG_ADMIN_USER};Password=${PG_PASSWORD};SSL Mode=Require;Trust Server Certificate=true"

# ── Step 3: Azure Container Registry ───────────────────────
echo "▸ [3/8] Creating Azure Container Registry..."
az acr create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$ACR_NAME" \
  --sku Basic \
  --admin-enabled true \
  --output none

ACR_LOGIN_SERVER=$(az acr show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$ACR_NAME" \
  --query loginServer -o tsv)

# ── Step 4: Build image in Azure (ACR Build Tasks — no local Docker needed)
echo "▸ [4/8] Building Docker image in Azure (ACR Build Tasks)..."
az acr build \
  --resource-group "$RESOURCE_GROUP" \
  --registry "$ACR_NAME" \
  --image "$CONTAINER_APP_NAME:$IMAGE_TAG" \
  --file Host/Musicratic.Host/Dockerfile \
  "$REPO_ROOT/src"

# ── Step 5: Container Apps Environment ──────────────────────
echo "▸ [5/8] Creating Container Apps environment..."
az containerapp env create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$CONTAINER_APP_ENV" \
  --location "$LOCATION" \
  --output none

# ── Step 6: Deploy Container App ───────────────────────────
echo "▸ [6/8] Deploying backend Container App..."

# Generate a JWT signing key for preview mode
PREVIEW_JWT_KEY="$(openssl rand -base64 48 | tr -d '/+=' | head -c 64)"

ACR_PASSWORD=$(az acr credential show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$ACR_NAME" \
  --query "passwords[0].value" -o tsv)

az containerapp create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$CONTAINER_APP_NAME" \
  --environment "$CONTAINER_APP_ENV" \
  --image "$ACR_LOGIN_SERVER/$CONTAINER_APP_NAME:$IMAGE_TAG" \
  --registry-server "$ACR_LOGIN_SERVER" \
  --registry-username "$ACR_NAME" \
  --registry-password "$ACR_PASSWORD" \
  --target-port 8080 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 1 \
  --cpu 0.5 \
  --memory 1Gi \
  --env-vars \
    "ASPNETCORE_ENVIRONMENT=Preview" \
    "Preview__Enabled=true" \
    "Preview__JwtSigningKey=$PREVIEW_JWT_KEY" \
    "ConnectionStrings__AuthDb=$PG_CONN_STRING" \
    "ConnectionStrings__HubDb=$PG_CONN_STRING" \
    "ConnectionStrings__PlaybackDb=$PG_CONN_STRING" \
    "ConnectionStrings__VotingDb=$PG_CONN_STRING" \
    "ConnectionStrings__EconomyDb=$PG_CONN_STRING" \
    "ConnectionStrings__AnalyticsDb=$PG_CONN_STRING" \
    "ConnectionStrings__SocialDb=$PG_CONN_STRING" \
    "ConnectionStrings__NotificationDb=$PG_CONN_STRING" \
    "MUSICRATIC_ENVIRONMENT=preview" \
  --output none

# Get the API URL
API_URL=$(az containerapp show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$CONTAINER_APP_NAME" \
  --query "properties.configuration.ingress.fqdn" -o tsv)
API_URL="https://$API_URL"

echo "  API deployed at: $API_URL"

# ── Step 7: Build Angular SPA ──────────────────────────────
echo "▸ [7/8] Building Angular SPA for Azure Static Web Apps..."
cd "$REPO_ROOT/web"

# Replace placeholder URLs with actual API URL
sed -i "s|PREVIEW_API_URL_PLACEHOLDER|$API_URL|g" src/environments/environment.preview.ts
sed -i "s|PREVIEW_WS_URL_PLACEHOLDER|${API_URL/https:/wss:}|g" src/environments/environment.preview.ts

npm ci
npx ng build --configuration preview

# ── Step 8: Deploy Static Web App ──────────────────────────
echo "▸ [8/8] Deploying Angular SPA to Azure Static Web Apps..."

az staticwebapp create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$STATIC_WEB_APP_NAME" \
  --location "$LOCATION" \
  --output none

SWA_TOKEN=$(az staticwebapp secrets list \
  --resource-group "$RESOURCE_GROUP" \
  --name "$STATIC_WEB_APP_NAME" \
  --query "properties.apiKey" -o tsv)

# Deploy using SWA CLI (install if needed)
npx --yes @azure/static-web-apps-cli deploy \
  "$REPO_ROOT/web/dist/musicratic-web/browser" \
  --deployment-token "$SWA_TOKEN" \
  --env production

SPA_URL=$(az staticwebapp show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$STATIC_WEB_APP_NAME" \
  --query "defaultHostname" -o tsv)
SPA_URL="https://$SPA_URL"

# ── Summary ─────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════╗"
echo "║  ✅ Deployment complete!                                     ║"
echo "╠══════════════════════════════════════════════════════════════╣"
echo "║                                                              ║"
echo "  🌐 Web App (SPA):  $SPA_URL"
echo "  🔌 API (Backend):  $API_URL"
echo "  📖 Swagger/Scalar: $API_URL/scalar/v1"
echo "  ❤️  Health Check:   $API_URL/health"
echo "║                                                              ║"
echo "║  Preview Auth (no Authentik needed):                         ║"
echo "  POST $API_URL/api/preview/auth/token"
echo '  Body: {"userId":"00000000-0000-0000-0000-000000000001"}'
echo "║                                                              ║"
echo "║  Credentials (SAVE THESE):                                   ║"
echo "  PostgreSQL Host:     $PG_HOST"
echo "  PostgreSQL User:     $PG_ADMIN_USER"
echo "  PostgreSQL Password: $PG_PASSWORD"
echo "  JWT Signing Key:     $PREVIEW_JWT_KEY"
echo "║                                                              ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""
echo "To tear down:  az group delete --name $RESOURCE_GROUP --yes --no-wait"
