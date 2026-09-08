# ==============================================================================
# Publish AAQSOLS Heart Clinic HMS for Windows Server
# ==============================================================================
$ErrorActionPreference = "Stop"

$rootDir = Split-Path $PSScriptRoot -Parent
$webDir = Join-Path $rootDir "src\Web"
$apiDir = Join-Path $rootDir "src\Api"
$outputDir = Join-Path $rootDir "publish\HeartClinicHms"
$wwwrootDir = Join-Path $outputDir "wwwroot"

Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host " Publishing AAQSOLS Heart Clinic HMS for Windows Server" -ForegroundColor Cyan
Write-Host "========================================================`n" -ForegroundColor Cyan

# 1. Build React Web UI
Write-Host "[1/4] Building React frontend..." -ForegroundColor Yellow
Set-Location $webDir
if (-not (Test-Path "node_modules")) {
    & npm.cmd install
}
& npm.cmd run build
Write-Host "[+] React build complete." -ForegroundColor Green

# 2. Publish .NET 10 API
Write-Host "`n[2/4] Publishing .NET 10 API..." -ForegroundColor Yellow
Set-Location $rootDir
dotnet publish "src\Api\HeartClinicHms.Api.csproj" -c Release -o $outputDir --nologo
Write-Host "[+] .NET API publish complete." -ForegroundColor Green

# 3. Copy React build into API wwwroot
Write-Host "`n[3/4] Packaging React UI into API wwwroot..." -ForegroundColor Yellow
if (Test-Path $wwwrootDir) {
    Remove-Item $wwwrootDir -Recurse -Force
}
New-Item -ItemType Directory -Path $wwwrootDir -Force | Out-Null
Copy-Item -Path (Join-Path $webDir "dist\*") -Destination $wwwrootDir -Recurse -Force
Write-Host "[+] wwwroot bundled." -ForegroundColor Green

# 4. Create start.cmd launcher
Write-Host "`n[4/4] Creating launcher script..." -ForegroundColor Yellow
$startScript = @"
@echo off
title AAQSOLS Heart Clinic HMS
set ASPNETCORE_ENVIRONMENT=Demo
set UseSqlite=true
set ASPNETCORE_URLS=http://0.0.0.0:5080
echo ========================================================
echo  AAQSOLS Heart Clinic HMS is running!
echo  Access locally at:  http://localhost:5080
echo ========================================================
HeartClinicHms.Api.exe
pause
"@
Set-Content -Path (Join-Path $outputDir "start.cmd") -Value $startScript

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " BUILD SUCCESSFUL!" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host " Output Folder: $outputDir" -ForegroundColor White
Write-Host "`nTo run immediately, execute:" -ForegroundColor Yellow
Write-Host "  cd `"$outputDir`"" -ForegroundColor Cyan
Write-Host "  .\start.cmd`n" -ForegroundColor Cyan
