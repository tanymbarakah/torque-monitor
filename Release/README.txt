================================================================================
                                TORQUE MONITOR
      Premium Standalone Analogue Hardware Monitoring Widget for Windows 11
================================================================================

Version: 1.0.0 (Release x64)
Release Date: 19-Nov-2026
Developer & Architect: Tanym Barakah
Designation: Founder & CEO @ NIHAR TECHNOLOGIES
Professional Title: Software & App Developer
Organization: NIHAR TECHNOLOGIES
Official Website: https://nihartechnologies.com
Contact Email: tanymbarakah@nihartechnologies.com
Country: Bangladesh

Social & Professional Repositories:
  - GitHub: https://github.com/tanymbarakah
  - LinkedIn: https://www.linkedin.com/in/tanym-barakah/
  - Facebook: https://www.facebook.com/tanymbarakah.me

License & Copyright:
  - Copyright (C) 2026 Tanym Barakah. NIHAR TECHNOLOGIES. All rights reserved.
  - License: Personal Use License (Non-Commercial Freeware)

--------------------------------------------------------------------------------
1. OVERVIEW & DESIGN CONCEPT
--------------------------------------------------------------------------------
TORQUE MONITOR transforms real-time computer hardware statistics into six highly 
realistic, independent automotive analogue gauges floating directly on your 
Windows 11 desktop.

Inspired by premium Porsche 911 GT3 instrument clusters, BMW M-series 
tachometers, and Mercedes-AMG performance instruments, each gauge operates as an 
independent, genuinely transparent desktop widget.

Key Visual Attributes:
- Circular deep-black instrument faces with subtle radial vignetting.
- Multi-stop chamfered brushed metallic outer bezels with chrome rim highlights.
- Razor-sharp automotive fluorescent red needles with realistic inertia and damping.
- Machined metallic center pivot caps with concentric ring etchings.
- Detailed major and minor white tick marks on calibrated 240-degree arcs.
- Dynamic illuminated accent scale zones (Cyan, Red, Emerald, Amber).
- Large bold automotive digital readouts and secondary telemetry.
- Embedded live waveform activity graphs with glowing illuminated polyline traces.

ABSOLUTELY NO:
- Cockpit or interior car mockups
- Background boxes or rectangular window frames
- Obtrusive visual containers
- Fake or simulated sensor readings

--------------------------------------------------------------------------------
2. PRIMARY GAUGES
--------------------------------------------------------------------------------
TOP ROW:
  01. CPU USAGE (0–100%)
      - Real-time CPU utilisation percentage
      - Current CPU clock frequency (GHz)
      - CPU model branding
      - Cyan / Ice-Blue illuminated scale
      - Live rolling CPU activity graph

  02. CPU TEMPERATURE (0–120°C)
      - Actual CPU core / package temperature in Celsius
      - Blue (<50°C), Yellow (50–80°C), and Red (80–120°C) thermal warning zones
      - Live temperature history graph

  03. GPU USAGE (0–100%)
      - Dedicated GPU core utilisation percentage
      - GPU core clock frequency (GHz)
      - GPU model branding (NVIDIA RTX / AMD Radeon)
      - Sapphire Cyan / Redline scale
      - Live rolling GPU activity graph

BOTTOM ROW:
  04. GPU TEMPERATURE (0–120°C)
      - Actual GPU core temperature in Celsius
      - GPU Hotspot temperature telemetry (when available)
      - Blue, Yellow, and Red thermal warning zones
      - Live temperature history graph

  05. RAM USAGE (0–100%)
      - System physical memory utilisation percentage
      - Used / Total RAM in Gigabytes (e.g., 10.9 / 16.0 GB)
      - High-performance Emerald / Mint illuminated scale
      - Live memory activity graph

  06. DISK ACTIVITY (0–100% Active Time)
      - Physical disk active time percentage
      - Independent Read and Write transfer speeds (MB/s)
      - Primary storage drive identification
      - Amber / Orange / Red illuminated scale
      - Live disk activity history graph

OPTIONAL GAUGES (Configurable via Context Menu):
  - Network Speed (Mbps)
  - CPU Clock Frequency (0–6 GHz)
  - GPU Core Clock (0–3 GHz)
  - System Uptime (Days / Hours)

--------------------------------------------------------------------------------
3. INTERACTION & CONTROLS
--------------------------------------------------------------------------------
- INDEPENDENT DRAGGING:
  Click and drag any individual gauge face with the left mouse button to position 
  it anywhere across single or multi-monitor setups. Other gauges remain fixed.

- POSITION PERSISTENCE:
  Every gauge remembers its exact X/Y screen coordinates upon release. Settings are 
  saved to %APPDATA%\TorqueMonitor\settings.json and restored upon restart. Multi-monitor 
  safety clamping ensures gauges are never lost off-screen.

- SMOOTH RESIZING:
  Hold CTRL and scroll your Mouse Wheel over any gauge to scale it smoothly 
  between 60% and 200%. Proportions, vector ticks, typography, and graphs scale 
  with pixel-perfect vector sharpness.

- DEVELOPER & LICENSE DIALOG:
  Access through the (i) button in settings/diagnostics or through the right-click 
  context menu and system tray menu ("Developer & License Information...").

- AUTOMOTIVE CONTEXT MENU (Right-Click on any Gauge):
  * Hide / Show Individual Gauges
  * Show All 6 Primary Gauges
  * Gauge Visibility Submenu
  * Gauge Size Presets (Small 80%, Standard 100%, Large 120%, Extra Large 140%)
  * Window Opacity (100%, 90%, 80%, 65%, 50%)
  * Lock Gauge Positions (prevents accidental movement)
  * Reset to 3x2 Grid Layout (cleanly centered on primary monitor)
  * Always On Top toggle
  * Refresh Rate selection (500 ms, 1000 ms, 2000 ms)
  * Start with Windows toggle
  * Hardware Sensor Information (live sensor diagnostics window)
  * Developer & License Information (Tanym Barakah - NIHAR TECHNOLOGIES)
  * Exit Application

- SYSTEM TRAY INTEGRATION:
  * Double-click tray icon to show/restore all gauges.
  * Right-click tray icon to access full application controls, settings, and 
    developer information.

--------------------------------------------------------------------------------
4. INSTALLATION & USAGE
--------------------------------------------------------------------------------
OPTION A: STANDALONE EXECUTABLE (PORTABLE)
  1. Simply run 'TorqueMonitor.exe'.
  2. No installation or .NET runtime installation required (fully self-contained).
  3. Settings are saved in your user AppData directory.

OPTION B: WINDOWS MSI INSTALLER
  1. Double-click 'TorqueMonitor-Setup-x64.msi'.
  2. Follow the setup wizard to install to Program Files.
  3. Shortcuts are automatically created in the Start Menu and on your Desktop.
  4. Fully integrated uninstaller available via Windows Settings > Installed Apps.

--------------------------------------------------------------------------------
5. HARDWARE MONITORING ENGINE & DIGITAL SIGNATURES
--------------------------------------------------------------------------------
Powered by LibreHardwareMonitorLib with automatic multi-tiered fallback:
  - Primary: LibreHardwareMonitor Ring-0 sensor interfaces
  - Secondary: High-precision Windows Performance Counters
  - Tertiary: Win32 native APIs (GlobalMemoryStatusEx, GetSystemTimes)

Digital Signature:
  - Subject: CN=Tanym Barakah, O=NIHAR TECHNOLOGIES, OU=Software Development, C=BD
  - Issuer: CN=NIHAR TECHNOLOGIES Root CA, O=NIHAR TECHNOLOGIES, C=BD
  - Code Signing Thumbprint: 0D38A7932F2798E8549DF7CD569BCB7BD4DF9571
  - Root CA Thumbprint: DD6130B9438DF95ED1EA494032219C3BAEF0E624
  - Authenticode SHA256 with DigiCert RFC 3161 Timestamping

Hashes:
  - TorqueMonitor-Setup-x64.msi SHA-256: 84A6BE8DBBD40185249AB9884BAD4D18663C185536DFB5DC1F89FBFD2AC55FFD
  - TorqueMonitor-v1.0.0-Portable.zip SHA-256: E879BCEF2D4DCF685C215E4F9D8A3BB85BB82A9D27A7823A287208DC6ED75F22
  - TorqueMonitor.exe SHA-256: 7C2D7F5D50A6E7E2EAD830E190BD405E7F0E89767950C6DF5A7EB142B2224F1F

Microsoft SmartScreen Notes:
  - If Windows Defender SmartScreen shows "Windows protected your PC", click "More info" and then "Run anyway".
  - Alternatively, the MSI installer automatically registers the publisher certificates into Windows Trusted Stores.
  - For portable usage, run Run-TorqueMonitor.cmd or run Install-Certificate.cmd as Administrator.

If a particular sensor is unavailable on a specific motherboard/CPU without 
elevated privileges, the gauge displays 'N/A' safely and honestly without 
fabricating fake readings or crashing.

================================================================================
(C) 2026 Tanym Barakah. NIHAR TECHNOLOGIES. All rights reserved.
Official Website: https://nihartechnologies.com
================================================================================
