# Azure Zero-Touch VM Deployment Guide

Deploy **AAQSOLS Heart Clinic HMS** to an **Azure Linux VM (Ubuntu 24.04 LTS)** with a single command.

---

## What the Zero-Touch Script Does Automatically

1. Creates an Azure Resource Group (`rg-heartclinic`).
2. Provisions an **Ubuntu 24.04 LTS VM** (`Standard_B2s`, 2 vCPU, 4GB RAM — covered by Azure Free Credits).
3. Executes `cloud-init.yaml` at boot:
   - Installs official Docker Engine & Docker Compose.
   - Clones the repository.
   - Spins up Microsoft SQL Server 2022 Linux, .NET 10 Web API, and React Web Nginx in `docker-compose.yml`.
4. Opens Network Security Group ports **80 (HTTP)** and **443 (HTTPS)**.
5. Outputs the public IP address of your clinic portal.

---

## Method 1: Using Azure Cloud Shell (Easiest — No Local Tools Needed)

You don't need to install anything on your PC! Azure Cloud Shell runs directly in your browser with Azure CLI pre-installed:

1. Open **[shell.azure.com](https://shell.azure.com)** (or click the **>_** Cloud Shell icon at the top of the [Azure Portal](https://portal.azure.com)).
2. Choose **Bash**.
3. Run:
   ```bash
   git clone -b qasimkhanfaridi-turbo-funicular https://github.com/qasimkhanfaridi/AAQSOLS-HeartClinic-HMS.git
   cd AAQSOLS-HeartClinic-HMS
   chmod +x scripts/deploy-azure-vm.sh
   ./scripts/deploy-azure-vm.sh
   ```
4. Wait ~2 minutes. The terminal will display:
   ```
   ========================================================
    DEPLOYMENT TRIGGERED SUCCESSFULLY!
   ========================================================
    Public IP:     20.xx.xx.xx
    Clinic URL:    http://20.xx.xx.xx
    Health Check:  http://20.xx.xx.xx/health
   ```
5. Allow ~3 minutes for the containers to finish building, then open `http://20.xx.xx.xx` in your browser!

---

## Method 2: From Local Windows PowerShell

1. Install Azure CLI on Windows (if not already installed):
   ```cmd
   winget install -e --id Microsoft.AzureCLI
   ```
   *(Restart PowerShell after installation).*

2. Log in to Azure:
   ```powershell
   az login
   ```

3. Run the zero-touch script:
   ```powershell
   .\scripts\deploy-azure-vm.ps1
   ```

4. The script provisions everything and outputs the live URL.

---

## Demo Credentials

Once the VM is up, log in with any of the seeded roles:

| Role | Username | Password |
|---|---|---|
| **Administrator** | `admin` | `Admin@123` |
| **Reception** | `reception` | `Reception@123` |
| **Doctor** | `doctor` | `Doctor@123` |

---

## Managing Your VM

- **Check Container Status (SSH into VM):**
  ```bash
  ssh azureuser@<public-ip>
  cd /opt/hms
  sudo docker compose ps
  sudo docker compose logs -f
  ```
- **Stop VM when not in use (to save credits):**
  ```powershell
  az vm deallocate --resource-group rg-heartclinic --name vm-heartclinic
  ```
- **Start VM again:**
  ```powershell
  az vm start --resource-group rg-heartclinic --name vm-heartclinic
  ```
- **Delete everything (clean teardown):**
  ```powershell
  az group delete --name rg-heartclinic --yes --no-wait
  ```
