@echo off
title Launching TORQUE MONITOR
echo =====================================================================
echo    TORQUE MONITOR — Windows 11 Hardware Monitor Widget
echo    Author: Tanym Barakah | NIHAR TECHNOLOGIES
echo =====================================================================
echo.
echo Unblocking file and starting widget...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Unblock-File '%~dp0TorqueMonitor.exe' -ErrorAction SilentlyContinue" >nul 2>&1
start "" "%~dp0TorqueMonitor.exe"
exit
