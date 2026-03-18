# ──────────────────────────────────────────────────────────────
# Musicratic — Azure Preview Deployment (PowerShell)
# Deploys: Container Apps (API) + PostgreSQL Flexible + Static Web Apps (SPA)
#
# Prerequisites:
#   - Azure CLI: winget install Microsoft.AzureCLI
#   - Node.js + npm
#   - Run: az login
#   (No Docker/Podman needed — image is built in Azure via ACR Build Tasks)
#
# Usage:
#   .\infra\azure\deploy-preview.ps1
# ──────────────────────────────────────────────────────────────
$ErrorActionPreference = "Stop"

# ── Refresh PATH (picks up tools installed via winget) ──────
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# ── Register required providers (idempotent) ────────────────
$providers = @("Microsoft.DBforPostgreSQL", "Microsoft.ContainerRegistry", "Microsoft.App", "Microsoft.OperationalInsights", "Microsoft.Web")
foreach ($p in $providers) {
  $state = cmd /c "az provider show -n $p --query registrationState -o tsv 2>&1"
  if ($state -ne "Registered") {
    Write-Host "Registering provider $p ..." -ForegroundColor Yellow
    az provider register -n $p --wait | Out-Null
  }
}

# ── Configuration ───────────────────────────────────────────
$ResourceGroup    = if ($env:MUSICRATIC_RG)       { $env:MUSICRATIC_RG }       else { "musicratic-preview-rg" }
$Location         = if ($env:MUSICRATIC_LOCATION)  { $env:MUSICRATIC_LOCATION }  else { "westeurope" }
$PgServerName     = if ($env:MUSICRATIC_PG_SERVER) { $env:MUSICRATIC_PG_SERVER } else { "musicratic-pg-preview" }
$PgAdminUser      = if ($env:MUSICRATIC_PG_USER)   { $env:MUSICRATIC_PG_USER }   else { "musicratic_admin" }
$PgDbName         = if ($env:MUSICRATIC_PG_DB)     { $env:MUSICRATIC_PG_DB }     else { "musicratic" }
$AcrName          = if ($env:MUSICRATIC_ACR)       { $env:MUSICRATIC_ACR }       else { "musicraticpreview" }
$ContainerAppEnv  = if ($env:MUSICRATIC_CAE)       { $env:MUSICRATIC_CAE }       else { "musicratic-preview-env" }
$ContainerAppName = if ($env:MUSICRATIC_CA)        { $env:MUSICRATIC_CA }        else { "musicratic-api" }
$StaticWebAppName = if ($env:MUSICRATIC_SWA)       { $env:MUSICRATIC_SWA }       else { "musicratic-web-preview" }
$ImageTag         = "latest"

$RepoRoot = (Resolve-Path "$PSScriptRoot\..\..").Path

Write-Host ""
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "  Musicratic — Azure Preview Deployment" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resource Group:  $ResourceGroup"
Write-Host "Location:        $Location"
Write-Host "PostgreSQL:      $PgServerName"
Write-Host "Container App:   $ContainerAppName"
Write-Host "Static Web App:  $StaticWebAppName"
Write-Host ""

# ── Step 1: Resource Group ──────────────────────────────────
Write-Host "[1/8] Creating resource group..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location --output none

# ── Step 2: PostgreSQL Flexible Server ──────────────────────
Write-Host "[2/8] Creating PostgreSQL Flexible Server (Burstable B1ms)..." -ForegroundColor Yellow

# Generate random password
$PgPassword = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 28 | ForEach-Object { [char]$_ }) + "Aa1!"

az postgres flexible-server create `
  --resource-group $ResourceGroup `
  --name $PgServerName `
  --location $Location `
  --admin-user $PgAdminUser `
  --admin-password $PgPassword `
  --sku-name Standard_B1ms `
  --tier Burstable `
  --storage-size 32 `
  --version 16 `
  --yes `
  --output none

# Allow Azure services
az postgres flexible-server firewall-rule create `
  --resource-group $ResourceGroup `
  --name $PgServerName `
  --rule-name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0 `
  --output none

# Create database
az postgres flexible-server db create `
  --resource-group $ResourceGroup `
  --server-name $PgServerName `
  --database-name $PgDbName `
  --output none

$PgHost = "$PgServerName.postgres.database.azure.com"
$PgConnString = "Host=$PgHost;Port=5432;Database=$PgDbName;Username=$PgAdminUser;Password=$PgPassword;SSL Mode=Require;Trust Server Certificate=true"

# ── Step 3: Azure Container Registry ───────────────────────
Write-Host "[3/8] Creating Azure Container Registry..." -ForegroundColor Yellow
az acr create `
  --resource-group $ResourceGroup `
  --name $AcrName `
  --sku Basic `
  --admin-enabled true `
  --output none

$AcrLoginServer = az acr show `
  --resource-group $ResourceGroup `
  --name $AcrName `
  --query loginServer -o tsv

# ── Step 4: Build image in Azure (ACR Build Tasks — no local Docker needed)
Write-Host "[4/8] Building Docker image in Azure (ACR Build Tasks)..." -ForegroundColor Yellow
Push-Location "$RepoRoot/src"
az acr build `
  --resource-group $ResourceGroup `
  --registry $AcrName `
  --image "${ContainerAppName}:$ImageTag" `
  --file Host/Musicratic.Host/Dockerfile `
  .
Pop-Location

# ── Step 5: Container Apps Environment ──────────────────────
Write-Host "[5/8] Creating Container Apps environment..." -ForegroundColor Yellow
az containerapp env create `
  --resource-group $ResourceGroup `
  --name $ContainerAppEnv `
  --location $Location `
  --output none

# ── Step 6: Deploy Container App ───────────────────────────
Write-Host "[6/8] Deploying backend Container App..." -ForegroundColor Yellow

# Generate JWT signing key
$PreviewJwtKey = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object { [char]$_ })

$AcrPassword = az acr credential show `
  --resource-group $ResourceGroup `
  --name $AcrName `
  --query "passwords[0].value" -o tsv

az containerapp create `
  --resource-group $ResourceGroup `
  --name $ContainerAppName `
  --environment $ContainerAppEnv `
  --image "$AcrLoginServer/${ContainerAppName}:$ImageTag" `
  --registry-server $AcrLoginServer `
  --registry-username $AcrName `
  --registry-password $AcrPassword `
  --target-port 8080 `
  --ingress external `
  --min-replicas 0 `
  --max-replicas 1 `
  --cpu 0.5 `
  --memory 1Gi `
  --env-vars `
    "ASPNETCORE_ENVIRONMENT=Preview" `
    "Preview__Enabled=true" `
    "Preview__JwtSigningKey=$PreviewJwtKey" `
    "ConnectionStrings__AuthDb=$PgConnString" `
    "ConnectionStrings__HubDb=$PgConnString" `
    "ConnectionStrings__PlaybackDb=$PgConnString" `
    "ConnectionStrings__VotingDb=$PgConnString" `
    "ConnectionStrings__EconomyDb=$PgConnString" `
    "ConnectionStrings__AnalyticsDb=$PgConnString" `
    "ConnectionStrings__SocialDb=$PgConnString" `
    "ConnectionStrings__NotificationDb=$PgConnString" `
    "MUSICRATIC_ENVIRONMENT=preview" `
  --output none

$ApiFqdn = az containerapp show `
  --resource-group $ResourceGroup `
  --name $ContainerAppName `
  --query "properties.configuration.ingress.fqdn" -o tsv
$ApiUrl = "https://$ApiFqdn"

Write-Host "  API deployed at: $ApiUrl" -ForegroundColor Green

# ── Step 7: Build Angular SPA ──────────────────────────────
Write-Host "[7/8] Building Angular SPA..." -ForegroundColor Yellow
Push-Location "$RepoRoot\web"

# Replace placeholder URLs
$envFile = Get-Content "src\environments\environment.preview.ts" -Raw
$envFile = $envFile -replace "PREVIEW_API_URL_PLACEHOLDER", $ApiUrl
$WsUrl = $ApiUrl -replace "https:", "wss:"
$envFile = $envFile -replace "PREVIEW_WS_URL_PLACEHOLDER", $WsUrl
Set-Content "src\environments\environment.preview.ts" $envFile

npm ci
npx ng build --configuration preview

Pop-Location

# ── Step 8: Deploy Static Web App ──────────────────────────
Write-Host "[8/8] Deploying Angular SPA to Azure Static Web Apps..." -ForegroundColor Yellow

az staticwebapp create `
  --resource-group $ResourceGroup `
  --name $StaticWebAppName `
  --location $Location `
  --output none

$SwaToken = az staticwebapp secrets list `
  --resource-group $ResourceGroup `
  --name $StaticWebAppName `
  --query "properties.apiKey" -o tsv

npx --yes @azure/static-web-apps-cli deploy `
  "$RepoRoot\web\dist\musicratic-web\browser" `
  --deployment-token $SwaToken `
  --env production

$SpaHostname = az staticwebapp show `
  --resource-group $ResourceGroup `
  --name $StaticWebAppName `
  --query "defaultHostname" -o tsv
$SpaUrl = "https://$SpaHostname"

# ── Summary ─────────────────────────────────────────────────
Write-Host ""
Write-Host "====================================================" -ForegroundColor Green
Write-Host "  Deployment complete!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Web App (SPA):   $SpaUrl" -ForegroundColor White
Write-Host "  API (Backend):   $ApiUrl" -ForegroundColor White
Write-Host "  Swagger/Scalar:  $ApiUrl/scalar/v1" -ForegroundColor White
Write-Host "  Health Check:    $ApiUrl/health" -ForegroundColor White
Write-Host ""
Write-Host "  Preview Auth (no Authentik needed):" -ForegroundColor White
Write-Host "  POST $ApiUrl/api/preview/auth/token" -ForegroundColor White
Write-Host '  Body: {"userId":"00000000-0000-0000-0000-000000000001"}' -ForegroundColor White
Write-Host ""
Write-Host "  Credentials (SAVE THESE):" -ForegroundColor Red
Write-Host "  PostgreSQL Host:     $PgHost" -ForegroundColor White
Write-Host "  PostgreSQL User:     $PgAdminUser" -ForegroundColor White
Write-Host "  PostgreSQL Password: $PgPassword" -ForegroundColor White
Write-Host "  JWT Signing Key:     $PreviewJwtKey" -ForegroundColor White
Write-Host ""
Write-Host "  To tear down:" -ForegroundColor Yellow
Write-Host "  az group delete --name $ResourceGroup --yes --no-wait" -ForegroundColor Yellow
