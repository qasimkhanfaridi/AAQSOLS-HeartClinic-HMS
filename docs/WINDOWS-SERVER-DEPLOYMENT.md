# Deploying AAQSOLS Heart Clinic HMS to an Existing Windows Server VM

Since you already have an Azure Windows Server VM up and running, you can host the Heart Clinic HMS directly on it alongside your other sites.

---

## Architecture: All-in-One Standalone Hosting

The .NET 10 API has been configured to **serve the React frontend directly from `wwwroot`**.

This means:
- **1 Single Process / Port:** You run `HeartClinicHms.Api.exe` on port 5080 (or port 80).
- **Zero CORS Issues:** Frontend and API share the exact same host and port.
- **Embedded Database:** Uses embedded SQLite (`HeartClinicDemo.db`) with zero external database setup required, or connects directly to local SQL Server.

---

## Method 1: Build & Run on Windows Server (Easiest & Fastest)

Connect to your Windows Server via Remote Desktop (RDP).

### Step 1: Open PowerShell on the Windows Server
Run:
```powershell
git clone -b qasimkhanfaridi-turbo-funicular https://github.com/qasimkhanfaridi/AAQSOLS-HeartClinic-HMS.git C:\Apps\HeartClinicHms
cd C:\Apps\HeartClinicHms
```

### Step 2: Build the Package
Run the automated build script:
```powershell
.\scripts\publish-windows-server.ps1
```
*(This compiles the React UI and .NET API into a single `publish\HeartClinicHms` folder).*

### Step 3: Run the Clinic HMS
```powershell
cd publish\HeartClinicHms
.\start.cmd
```
The application will launch on `http://0.0.0.0:5080`.

---

## Method 2: Host via IIS (Internet Information Services)

If you already use IIS on your Windows Server for your other websites:

1. Install the **ASP.NET Core Hosting Bundle** (if not already installed).
2. Open **IIS Manager** &rarr; **Sites** &rarr; **Add Website**:
   - **Site name:** `HeartClinicHms`
   - **Physical path:** `C:\Apps\HeartClinicHms\publish\HeartClinicHms`
   - **Port:** `5080` (or `80` with a host header like `hms.yourcompany.com`).
3. Set the Application Pool to **No Managed Code**.
4. Start the site.

---

## Azure Firewall / Networking Requirement

In the Azure Portal, ensure port **5080** (or **80**) is open for incoming traffic:

1. Go to your Windows Server VM in the [Azure Portal](https://portal.azure.com).
2. Click **Networking** (or **Network settings**).
3. Click **Add inbound port rule**:
   - **Destination port ranges:** `5080` (or `80`)
   - **Protocol:** `TCP`
   - **Action:** `Allow`
   - **Name:** `Port_5080_HMS`
4. Click **Add**.

---

## Accessing Your Clinic

Open your browser and navigate to:
```
http://<YOUR-WINDOWS-SERVER-PUBLIC-IP>:5080
```

### Demo Logins
| Role | Username | Password |
|---|---|---|
| **Administrator** | `admin` | `Admin@123` |
| **Reception** | `reception` | `Reception@123` |
| **Doctor** | `doctor` | `Doctor@123` |
