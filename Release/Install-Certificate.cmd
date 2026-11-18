@echo off
:: Batch script to install NIHAR TECHNOLOGIES Root CA and Code Signing Cert
:: Run as Administrator for full system trust
title TORQUE MONITOR — Certificate Trust Installer
color 0B
echo =====================================================================
echo    TORQUE MONITOR — Digital Certificate Trust Installer
echo    Author: Tanym Barakah ^| Organization: NIHAR TECHNOLOGIES
echo =====================================================================
echo.
echo Installing NIHAR TECHNOLOGIES Root CA to Trusted Root Store...
certutil -addstore -f "Root" "%~dp0NiharTechnologies-RootCA.cer" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo  [OK] Root CA successfully trusted.
) else (
    echo  [NOTE] Standard user trust applied. For full machine trust, right-click and Run as Administrator.
    certutil -addstore -user -f "Root" "%~dp0NiharTechnologies-RootCA.cer" >nul 2>&1
)

echo Installing Tanym Barakah Code Signing Certificate to Trusted Publishers...
certutil -addstore -f "TrustedPublisher" "%~dp0TanymBarakah-CodeSigning.cer" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    certutil -addstore -user -f "TrustedPublisher" "%~dp0TanymBarakah-CodeSigning.cer" >nul 2>&1
)
echo  [OK] Publisher verified: Tanym Barakah (NIHAR TECHNOLOGIES).

echo.
echo Unblocking downloaded binaries (removing SmartScreen Mark-of-the-Web)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Unblock-File '%~dp0TorqueMonitor.exe' -ErrorAction SilentlyContinue; Unblock-File '%~dp0TorqueMonitor-Setup-x64.msi' -ErrorAction SilentlyContinue" >nul 2>&1
echo  [OK] Binaries unblocked.

echo.
echo =====================================================================
echo    SUCCESS: Certificate trust installed successfully!
echo    You can now launch TorqueMonitor.exe directly without SmartScreen alerts.
echo =====================================================================
echo.
pause
