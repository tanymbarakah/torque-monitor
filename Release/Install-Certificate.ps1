# PowerShell script to install NIHAR TECHNOLOGIES Root CA and Code Signing Cert
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "   TORQUE MONITOR — Digital Certificate Trust Installer" -ForegroundColor Cyan
Write-Host "   Author: Tanym Barakah | Organization: NIHAR TECHNOLOGIES" -ForegroundColor Cyan
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host ""

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootCer = Join-Path $scriptDir "NiharTechnologies-RootCA.cer"
$csCer = Join-Path $scriptDir "TanymBarakah-CodeSigning.cer"

if (Test-Path $rootCer) {
    try {
        certutil -addstore -f "Root" $rootCer | Out-Null
        Write-Host " [OK] NIHAR TECHNOLOGIES Root CA successfully trusted." -ForegroundColor Green
    } catch {
        certutil -addstore -user -f "Root" $rootCer | Out-Null
        Write-Host " [OK] Root CA added to CurrentUser trust." -ForegroundColor Yellow
    }
}

if (Test-Path $csCer) {
    try {
        certutil -addstore -f "TrustedPublisher" $csCer | Out-Null
        Write-Host " [OK] Tanym Barakah Code Signing Certificate trusted." -ForegroundColor Green
    } catch {
        certutil -addstore -user -f "TrustedPublisher" $csCer | Out-Null
        Write-Host " [OK] Publisher certificate added to CurrentUser trust." -ForegroundColor Yellow
    }
}

# Unblock files to remove Mark-of-the-Web
$exe = Join-Path $scriptDir "TorqueMonitor.exe"
$msi = Join-Path $scriptDir "TorqueMonitor-Setup-x64.msi"
if (Test-Path $exe) { Unblock-File -Path $exe -ErrorAction SilentlyContinue }
if (Test-Path $msi) { Unblock-File -Path $msi -ErrorAction SilentlyContinue }
Write-Host " [OK] Mark-of-the-Web unblocked." -ForegroundColor Green

Write-Host ""
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "   SUCCESS: Certificate trusted and files unblocked!" -ForegroundColor Green
Write-Host "   TorqueMonitor.exe can now be launched directly without warnings." -ForegroundColor Green
Write-Host "=====================================================================" -ForegroundColor Cyan
