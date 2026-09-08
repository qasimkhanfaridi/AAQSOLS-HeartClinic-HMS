# ==============================================================================
# AAQSOLS Heart Clinic HMS — Azure Zero-Touch VM Deployment Script
# ==============================================================================
param(
    [string]$ResourceGroup = "rg-heartclinic",
    [string]$Location = "eastus",
    [string]$VmName = "vm-heartclinic",
    [string]$VmSize = "Standard_B2s",        # 2 vCPUs, 4 GB RAM (~$30/month)
    [string]$AdminUser = "azureuser"
)

$ErrorActionPreference = "Stop"

Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host " AAQSOLS Heart Clinic HMS — Azure Zero-Touch Deployment" -ForegroundColor Cyan
Write-Host "========================================================`n" -ForegroundColor Cyan

# 1. Verify Azure CLI is installed
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "[-] Azure CLI ('az') is not installed." -ForegroundColor Red
    Write-Host "    Install it via: winget install -e --id Microsoft.AzureCLI" -ForegroundColor Yellow
    Write-Host "    Or run this script directly inside Azure Cloud Shell (https://shell.azure.com)`n" -ForegroundColor Yellow
    exit 1
}

# 2. Check Azure login status
Write-Host "[1/5] Checking Azure account..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "[-] Not logged in. Launching 'az login'..." -ForegroundColor Yellow
    az login
    $account = az account show | ConvertFrom-Json
}
Write-Host "[+] Logged in to Azure Subscription: $($account.name) ($($account.id))" -ForegroundColor Green

# 3. Create Resource Group
Write-Host "`n[2/5] Creating/verifying Resource Group '$ResourceGroup' in '$Location'..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location --output none
Write-Host "[+] Resource Group ready." -ForegroundColor Green

# 4. Check cloud-init file
$cloudInitPath = Join-Path $PSScriptRoot "..\cloud-init.yaml"
if (-not (Test-Path $cloudInitPath)) {
    Write-Host "[-] Error: cloud-init.yaml not found at $cloudInitPath" -ForegroundColor Red
    exit 1
}

# 5. Create Virtual Machine with Cloud-Init
Write-Host "`n[3/5] Deploying Ubuntu 24.04 LTS VM ($VmName) with zero-touch automation..." -ForegroundColor Yellow
Write-Host "      Size: $VmSize" -ForegroundColor Gray
Write-Host "      This step takes approximately 1 to 2 minutes..." -ForegroundColor Gray

az vm create `
    --resource-group $ResourceGroup `
    --name $VmName `
    --image Ubuntu2404 `
    --size $VmSize `
    --admin-username $AdminUser `
    --generate-ssh-keys `
    --custom-data $cloudInitPath `
    --public-ip-sku Standard `
    --output none

Write-Host "[+] Virtual Machine created successfully." -ForegroundColor Green

# 6. Open Web Firewall Ports (HTTP 80, HTTPS 443)
Write-Host "`n[4/5] Configuring Network Security Group rules (Ports 80 & 443)..." -ForegroundColor Yellow
az vm open-port --resource-group $ResourceGroup --name $VmName --port 80 --priority 1001 --output none
az vm open-port --resource-group $ResourceGroup --name $VmName --port 443 --priority 1002 --output none
Write-Host "[+] Web ports opened." -ForegroundColor Green

# 7. Retrieve Public IP
Write-Host "`n[5/5] Fetching Public IP address..." -ForegroundColor Yellow
$publicIp = az vm show -d -g $ResourceGroup -n $VmName --query publicIps -o tsv

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " DEPLOYMENT TRIGGERED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host " Public IP:     $publicIp" -ForegroundColor White
Write-Host " Clinic URL:    http://$publicIp" -ForegroundColor Cyan
Write-Host " Health Check:  http://$publicIp/health" -ForegroundColor Cyan
Write-Host " Admin User:    $AdminUser" -ForegroundColor White
Write-Host "`nNote: Cloud-Init is currently installing Docker and building" -ForegroundColor Gray
Write-Host "the HMS stack inside the VM. Please allow 3-4 minutes for the" -ForegroundColor Gray
Write-Host "containers to finish initializing before opening the URL.`n" -ForegroundColor Gray
