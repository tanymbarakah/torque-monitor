# TORQUE MONITOR — OFFICIAL USER MANUAL & OPERATION GUIDE
### Version 1.0.0 (Windows 11 x64 Edition)
**Developer:** Tanym Barakah (Founder & CEO @ NIHAR TECHNOLOGIES)  
**Official Organization:** NIHAR TECHNOLOGIES  
**Official Website:** [https://nihartechnologies.com](https://nihartechnologies.com)  
**Repository:** [https://github.com/tanymbarakah/torque-monitor](https://github.com/tanymbarakah/torque-monitor)

---

## 1. PRODUCT OVERVIEW

**TORQUE MONITOR** is a high-performance, standalone automotive hardware monitoring widget designed specifically for Windows 11 (with backwards compatibility for Windows 10 x64).

Inspired by the precision engineering of Porsche 911 GT3 instrument clusters, BMW M-series tachometers, and Mercedes-AMG performance instruments, TORQUE MONITOR turns computer telemetry into six independent, floating physical dials.

Unlike conventional system monitors, TORQUE MONITOR eliminates rectangular application windows, carbon-fibre dashboard backdrops, and artificial cockpits. Only the circular dials and their soft ambient shadows are rendered directly above your Windows desktop wallpaper.

---

## 2. SYSTEM REQUIREMENTS

| Component | Minimum Specification | Recommended Specification |
| :--- | :--- | :--- |
| **Operating System** | Windows 10 x64 (Build 19041+) | Windows 11 x64 (Version 22H2 or newer) |
| **Processor** | 64-bit Dual-Core 2.0 GHz | Modern AMD Ryzen or Intel Core i3/i5/i7/i9 |
| **Memory (RAM)** | 4 GB System RAM | 8 GB or higher |
| **Graphics** | DirectX 11 / WDDM 2.0 compatible | Dedicated NVIDIA GeForce RTX / AMD Radeon |
| **Storage** | 100 MB available drive space | NVMe Solid-State Drive |
| **Privileges** | Standard user account | Standard user account (No Admin required for normal use) |

---

## 3. INSTALLATION & DEPLOYMENT

TORQUE MONITOR provides two self-contained distribution options that require **no** external .NET SDK or runtime installations.

### Option A: Portable Standalone Executable (`TorqueMonitor.exe`)
1. Download `TorqueMonitor.exe` from the official GitHub Releases page.
2. Place the executable in any preferred folder (e.g., `C:\Tools\TorqueMonitor\`).
3. Double-click `TorqueMonitor.exe` to launch immediately.
4. Settings are automatically saved to `%APPDATA%\TorqueMonitor\settings.json`.

### Option B: Windows MSI Installer (`TorqueMonitor-Setup-x64.msi`)
1. Download `TorqueMonitor-Setup-x64.msi` from the official GitHub Releases page.
2. Double-click the `.msi` file and follow the standard Windows Installer wizard.
3. Shortcuts are created in your Start Menu and Desktop.
4. To uninstall, navigate to **Windows Settings > Apps > Installed Apps** and select **Uninstall**.

---

## 4. GAUGE TELEMETRY GUIDE

TORQUE MONITOR features six primary automotive analogue instruments arranged by default in a balanced 3-column × 2-row layout:

### TOP ROW

#### Gauge 01: CPU USAGE (0–100%)
- **Scale:** 0 to 100% utilisation on a 240° calibrated radial arc.
- **Illumination:** Cyan and Ice-Blue accent scale with fluorescent red needle.
- **Center Telemetry:** Current CPU load percentage (e.g., `42%`).
- **Secondary Telemetry:** Active processor clock frequency in GHz (e.g., `3.60 GHz`).
- **Waveform Trace:** Real-time 40-point rolling CPU activity history graph.

#### Gauge 02: CPU TEMPERATURE (0–120°C)
- **Scale:** 0 to 120°C thermal scale.
- **Thermal Zones:**
  - **Blue Low Zone (0–50°C):** Normal operating conditions.
  - **Yellow Mid Zone (50–80°C):** Moderate load under gaming or rendering.
  - **Red Alert Zone (80–120°C):** Thermal throttling threshold.
- **Center Telemetry:** Instantaneous package/core temperature reading in Celsius.
- **Waveform Trace:** Historical temperature variation waveform.

#### Gauge 03: GPU USAGE (0–100%)
- **Scale:** 0 to 100% dedicated graphics processor utilisation.
- **Illumination:** Vivid Sapphire-Cyan scale with Redline alert mark.
- **Center Telemetry:** Real-time GPU core compute load percentage (e.g., `78%`).
- **Secondary Telemetry:** Dedicated graphics core clock speed (GHz).
- **Waveform Trace:** Live graphics load history graph.

### BOTTOM ROW

#### Gauge 04: GPU TEMPERATURE (0–120°C)
- **Scale:** 0 to 120°C calibrated thermal range.
- **Thermal Zones:** Multi-colored Blue, Yellow, and Red alert segments.
- **Center Telemetry:** GPU core temperature in Celsius.
- **Secondary Telemetry:** Dedicated GPU Hotspot sensor reading when supported.
- **Waveform Trace:** Thermal history waveform.

#### Gauge 05: RAM USAGE (0–100%)
- **Scale:** 0 to 100% total system physical memory load.
- **Illumination:** High-performance Emerald and Mint-Green illuminated scale.
- **Center Telemetry:** RAM utilisation percentage (e.g., `68%`).
- **Secondary Telemetry:** Absolute memory load ratio (e.g., `10.9 / 16.0 GB`).
- **Waveform Trace:** Real-time memory allocation waveform.

#### Gauge 06: DISK ACTIVITY (0–100% Active Time)
- **Scale:** 0 to 100% physical drive active time.
- **Illumination:** Amber, Orange, and Red active time scale.
- **Center Telemetry:** Physical disk active percentage (e.g., `24%`).
- **Secondary Telemetry:** Independent live Read and Write transfer rates (e.g., `R: 120 | W: 85 MB/s`).
- **Waveform Trace:** Rolling storage drive throughput history graph.

---

## 5. MOUSE & GESTURE CONTROLS

TORQUE MONITOR provides an intuitive, borderless user interface:

| Action | Control Gesture | Description |
| :--- | :--- | :--- |
| **Move Individual Meter** | **Left Click & Drag** on dial face | Moves the selected gauge anywhere across your monitors. |
| **Resize Specific Meter** | **Scroll Mouse Wheel** over dial | Dynamically scales the targeted meter between 60% and 200%. |
| **Open Context Menu** | **Right-Click** on any dial face | Opens the dark automotive settings and customization menu. |
| **Reset 3x2 Grid** | Context Menu > **Reset to 3x2 Grid** | Re-centers all 6 meters into the default sports-car layout. |
| **Lock Positions** | Context Menu > **Lock Gauge Positions** | Prevents accidental dragging while maintaining interactivity. |
| **Adjust Opacity** | Context Menu > **Window Opacity** | Sets transparency level (100%, 90%, 80%, 65%, 50% Ghost). |
| **Toggle Visibility** | Context Menu > **Gauge Visibility** | Shows or hides individual meters to customize your desktop. |
| **Developer Info** | Click **(ⓘ)** or select from menu | Displays official developer, organization, and license details. |

---

## 6. SYSTEM TRAY INTEGRATION

The application runs quietly in your Windows 11 system tray with an automotive tachometer icon:
- **Double-Click Icon:** Instantly restores and reveals all 6 primary dials.
- **Right-Click Icon:** Opens the complete system tray control menu, including refresh interval adjustments, global sizing, startup settings, and diagnostic tools.
- **Exit Application:** Closes all floating instruments and gracefully unloads hardware monitoring sensors.

---

## 7. AUTHENTICITY, ANTI-PIRACY & CHECKSUMS

To protect users against unauthorized tampering, trojans, malicious re-packs, or piracy, always verify that your downloaded binaries match the official cryptographic hashes generated by the author:

### Official SHA-256 Release Hashes (v1.0.0)

```
TorqueMonitor.exe (Standalone x64)
SHA-256: 974A4B2408CDDBBE697A48825365713920A324D8C5726A892499FC5CA3DACB48

TorqueMonitor-Setup-x64.msi (Windows Installer x64)
SHA-256: 91CB3C7F40CEA8097E97455A0786267E18CF61FF6432968C6A7AA23FBEAF6832
```

#### How to Verify File Integrity in Windows PowerShell:
```powershell
Get-FileHash -Algorithm SHA256 "path\to\TorqueMonitor.exe"
```
Compare the resulting hash with the official values above. If the hashes do not match, the file has been modified and should not be executed.

---

## 8. LEGAL NOTICE & LICENSE

Copyright (C) 2026 **Tanym Barakah**. **NIHAR TECHNOLOGIES**. All Rights Reserved.

- **License Type:** Personal Use License (Non-Commercial Freeware)
- **Authorized Use:** You are granted a free, non-exclusive license to use this software on personal machines for private hardware monitoring and gaming observation.
- **Commercial Restrictions:** Commercial deployment, enterprise bundling, resale, or charging a fee for this software is strictly prohibited without explicit written permission from Tanym Barakah.
- **Trademark:** TORQUE MONITOR and the NIHAR TECHNOLOGIES branding are intellectual properties of Tanym Barakah.

---

## 9. TECHNICAL SUPPORT & CONTACT

- **Developer:** Tanym Barakah
- **Designation:** Founder & CEO @ NIHAR TECHNOLOGIES
- **Email:** [tanymbarakah@nihartechnologies.com](mailto:tanymbarakah@nihartechnologies.com)
- **Organization Website:** [https://nihartechnologies.com](https://nihartechnologies.com)
- **GitHub Repository:** [https://github.com/tanymbarakah/torque-monitor](https://github.com/tanymbarakah/torque-monitor)
