#!/bin/bash
# ==============================================================================
# AAQSOLS Heart Clinic HMS — Azure Zero-Touch VM Deployment Script (Bash)
# ==============================================================================
set -e

RESOURCE_GROUP="${1:-rg-heartclinic}"
LOCATION="${2:-eastus}"
VM_NAME="${3:-vm-heartclinic}"
VM_SIZE="${4:-Standard_B2s}"
ADMIN_USER="${5:-azureuser}"

echo -e "\n========================================================"
echo -e " AAQSOLS Heart Clinic HMS — Azure Zero-Touch Deployment"
echo -e "========================================================\n"

# 1. Verify Azure CLI
if ! command -v az &> /dev/null; then
    echo "[-] Azure CLI ('az') is not installed."
    echo "    Install via: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash"
    exit 1
fi

# 2. Check Azure login
echo "[1/5] Checking Azure account..."
if ! az account show &> /dev/null; then
    echo "[-] Not logged in. Running 'az login'..."
    az login
fi

# 3. Create Resource Group
echo -e "\n[2/5] Creating Resource Group '$RESOURCE_GROUP' in '$LOCATION'..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none
echo "[+] Resource Group ready."

# 4. Locate cloud-init
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CLOUD_INIT_PATH="$SCRIPT_DIR/../cloud-init.yaml"

if [ ! -f "$CLOUD_INIT_PATH" ]; then
    echo "[-] Error: cloud-init.yaml not found at $CLOUD_INIT_PATH"
    exit 1
fi

# 5. Create VM with Cloud-Init
echo -e "\n[3/5] Deploying Ubuntu 24.04 LTS VM ($VM_NAME)..."
az vm create \
    --resource-group "$RESOURCE_GROUP" \
    --name "$VM_NAME" \
    --image Ubuntu2404 \
    --size "$VM_SIZE" \
    --admin-username "$ADMIN_USER" \
    --generate-ssh-keys \
    --custom-data "$CLOUD_INIT_PATH" \
    --public-ip-sku Standard \
    --output none

echo "[+] Virtual Machine created successfully."

# 6. Open Web Firewall Ports
echo -e "\n[4/5] Opening firewall ports (80 HTTP, 443 HTTPS)..."
az vm open-port --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" --port 80 --priority 1001 --output none
az vm open-port --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" --port 443 --priority 1002 --output none
echo "[+] Web ports opened."

# 7. Retrieve Public IP
echo -e "\n[5/5] Fetching Public IP address..."
PUBLIC_IP=$(az vm show -d -g "$RESOURCE_GROUP" -n "$VM_NAME" --query publicIps -o tsv)

echo -e "\n========================================================"
echo -e " DEPLOYMENT TRIGGERED SUCCESSFULLY!"
echo -e "========================================================"
echo -e " Public IP:     $PUBLIC_IP"
echo -e " Clinic URL:    http://$PUBLIC_IP"
echo -e " Health Check:  http://$PUBLIC_IP/health"
echo -e "\nAllow ~3-4 minutes for Cloud-Init to initialize the containers."
