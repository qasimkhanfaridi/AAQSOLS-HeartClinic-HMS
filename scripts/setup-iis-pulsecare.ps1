# ==============================================================================
# AAQSOLS Heart Clinic HMS - Automated IIS Setup for Windows Server
# Configures: pulsecare.aaqsols.com on *:80 (http)
# ==============================================================================
# Run this script as Administrator on the Windows Server:
# powershell -ExecutionPolicy Bypass -File .\scripts\setup-iis-pulsecare.ps1
# ==============================================================================

param(
    [string]$SiteName = "pulsecare.aaqsols.com",
    [string]$HostHeader = "pulsecare.aaqsols.com",
    [int]$Port = 80,
    [string]$PhysicalPath = "C:\inetpub\wwwroot\pulsecare.aaqsols.com"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " AAQSOLS Heart Clinic HMS - IIS Server Auto-Setup" -ForegroundColor Cyan
Write-Host (' Target Domain: {0} on *:{1} (http)' -f $HostHeader, $Port) -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Administrator check
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[-] ERROR: This script must be run as Administrator." -ForegroundColor Red
    Write-Host "    Right-click PowerShell -> 'Run as Administrator', then run this script again." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

# 2. Verify IIS is installed
Write-Host "[1/6] Checking IIS Web Server feature..." -ForegroundColor Yellow
if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
    Write-Host "    Installing IIS Web Server and management tools..." -ForegroundColor Gray
    Install-WindowsFeature -Name Web-Server, Web-Static-Content, Web-Default-Doc, Web-Http-Errors -IncludeManagementTools | Out-Null
}
Import-Module WebAdministration -ErrorAction SilentlyContinue
Write-Host "[+] IIS WebAdministration module ready." -ForegroundColor Green

# 3. Check for ASP.NET Core Hosting Bundle
Write-Host ""
Write-Host "[2/6] Checking ASP.NET Core Module for IIS..." -ForegroundColor Yellow
$modulePath = "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
if (-not (Test-Path $modulePath)) {
    Write-Host "[-] ASP.NET Core Hosting Bundle not detected." -ForegroundColor Yellow
    Write-Host "    Downloading .NET 10 Hosting Bundle installer..." -ForegroundColor Gray
    $installerUrl = "https://aka.ms/dotnet/10.0/dotnet-hosting-win.exe"
    $installerPath = "$env:TEMP\dotnet-hosting.exe"
    Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath -UseBasicParsing
    Write-Host "    Installing Hosting Bundle (silent)..." -ForegroundColor Gray
    Start-Process -FilePath $installerPath -ArgumentList "/quiet /norestart" -Wait
    Write-Host "[+] Hosting Bundle installed. Restarting IIS..." -ForegroundColor Green
    & iisreset /restart | Out-Null
} else {
    Write-Host "[+] AspNetCoreModuleV2 is installed." -ForegroundColor Green
}

# 4. Build and publish package
Write-Host ""
Write-Host "[3/6] Publishing application bundle..." -ForegroundColor Yellow
$rootDir = Split-Path $PSScriptRoot -Parent
$publishScript = Join-Path $PSScriptRoot "publish-windows-server.ps1"
$publishSource = Join-Path $rootDir "publish\HeartClinicHms"

if (-not (Test-Path $publishSource)) {
    if (Test-Path $publishScript) {
        Write-Host "    Running build script..." -ForegroundColor Gray
        & powershell -ExecutionPolicy Bypass -File $publishScript
    } else {
        Write-Host "[-] Error: Neither publish folder nor build script found." -ForegroundColor Red
        exit 1
    }
}

# 5. Copy files to destination
Write-Host ""
Write-Host ('[4/6] Deploying files to {0}...' -f $PhysicalPath) -ForegroundColor Yellow
if (-not (Test-Path $PhysicalPath)) {
    New-Item -ItemType Directory -Path $PhysicalPath -Force | Out-Null
}
Copy-Item -Path (Join-Path $publishSource "*") -Destination $PhysicalPath -Recurse -Force
Write-Host "[+] Files copied successfully." -ForegroundColor Green

# 6. Set folder permissions for SQLite and IIS
Write-Host ""
Write-Host "[5/6] Configuring folder permissions for IIS and database..." -ForegroundColor Yellow
try {
    $acl = Get-Acl $PhysicalPath
    $ruleIisUsers = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
    $ruleAppPool = New-Object System.Security.AccessControl.FileSystemAccessRule(('IIS AppPool\{0}' -f $SiteName), "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.AddAccessRule($ruleIisUsers)
    $acl.AddAccessRule($ruleAppPool)
    Set-Acl $PhysicalPath $acl
    Write-Host ('[+] Permissions granted to IIS_IUSRS and IIS AppPool\{0}.' -f $SiteName) -ForegroundColor Green
} catch {
    & icacls $PhysicalPath /grant 'IIS_IUSRS:(OI)(CI)M' /T /Q | Out-Null
    Write-Host '[+] Permissions granted via icacls.' -ForegroundColor Green
}

# 7. Configure IIS Application Pool & Website
Write-Host ""
Write-Host "[6/6] Creating IIS Website and Application Pool..." -ForegroundColor Yellow

# App Pool
$appPoolPath = Join-Path 'IIS:\AppPools' $SiteName
if (Test-Path $appPoolPath) {
    Write-Host ('    Stopping existing App Pool {0}...' -f $SiteName) -ForegroundColor Gray
    Stop-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
} else {
    Write-Host ('    Creating App Pool {0}...' -f $SiteName) -ForegroundColor Gray
    New-Item -Path $appPoolPath -Force | Out-Null
}
Set-ItemProperty -Path $appPoolPath -Name 'managedRuntimeVersion' -Value ''
Set-ItemProperty -Path $appPoolPath -Name 'managedPipelineMode' -Value 0

# Website
$sitePath = Join-Path 'IIS:\Sites' $SiteName
if (Test-Path $sitePath) {
    Write-Host ('    Updating existing Website {0}...' -f $SiteName) -ForegroundColor Gray
    Stop-WebSite -Name $SiteName -ErrorAction SilentlyContinue
    Remove-Website -Name $SiteName
}

Write-Host ('    Binding: http on *:{0} with Host Header {1}...' -f $Port, $HostHeader) -ForegroundColor Gray
New-Website -Name $SiteName -Port $Port -HostHeader $HostHeader -PhysicalPath $PhysicalPath -ApplicationPool $SiteName | Out-Null

# Start
Start-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
Start-WebSite -Name $SiteName -ErrorAction SilentlyContinue

Write-Host ''
Write-Host '========================================================' -ForegroundColor Green
Write-Host ' IIS SETUP COMPLETED SUCCESSFULLY!' -ForegroundColor Green
Write-Host '========================================================' -ForegroundColor Green
Write-Host (' Site Name:     {0}' -f $SiteName) -ForegroundColor White
Write-Host (' Binding:       Browse {0} on *:{1} (http)' -f $HostHeader, $Port) -ForegroundColor Cyan
Write-Host (' Physical Path: {0}' -f $PhysicalPath) -ForegroundColor White
Write-Host (' App Pool:      {0} (No Managed Code)' -f $SiteName) -ForegroundColor White
Write-Host ""
Write-Host ('Ensure your DNS A-Record for {0} points to this Windows Server public IP address.' -f $HostHeader) -ForegroundColor Yellow
Write-Host ""
