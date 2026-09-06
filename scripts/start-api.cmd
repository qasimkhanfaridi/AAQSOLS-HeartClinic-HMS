@echo off
echo === PulseCore Heart Clinic HMS - Start API ===
echo.
taskkill /F /IM HeartClinicHms.Api.exe 2>nul
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5080" ^| findstr LISTENING') do taskkill /F /PID %%a 2>nul

set OUT=%~dp0..\run\api7
set ASPNETCORE_URLS=http://localhost:5080

cd /d %~dp0
dotnet build src\Api/HeartClinicHms.Api.csproj -c Debug -p:OutputPath="%OUT%\" -p:AppendTargetFrameworkToOutputPath=false
if errorlevel 1 exit /b 1

copy /Y src\Api\appsettings.json "%OUT%\"
cd /d "%OUT%"
echo.
echo API running at http://localhost:5080
echo Login: admin / Admin@123
echo.
dotnet HeartClinicHms.Api.dll
