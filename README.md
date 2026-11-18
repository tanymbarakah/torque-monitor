# TORQUE MONITOR
### Premium Standalone Analogue Hardware Monitoring Widget for Windows 11

[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20%7C%2010-0078D4?logo=windows)](https://microsoft.com)
[![Framework](https://img.shields.io/badge/Framework-.NET%208.0%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-Personal%20Use%20Freeware-10B981)](#license--copyright)
[![Author](https://img.shields.io/badge/Developer-Tanym%20Barakah-00E5FF)](https://github.com/tanymbarakah)
[![Organization](https://img.shields.io/badge/Organization-NIHAR%20TECHNOLOGIES-38BDF8)](https://nihartechnologies.com)

---

## 🏎️ Overview

**TORQUE MONITOR** transforms real-time computer hardware statistics into six highly realistic, independent automotive analogue gauges floating directly on your Windows 11 desktop.

Inspired by premium **Porsche 911 GT3** instrument clusters, **BMW M-series** tachometers, and **Mercedes-AMG** performance instruments, each gauge operates as an independent, genuinely transparent desktop widget.

There are **no** vehicle cockpits, **no** bulky rectangular dashboard panels, and **no** fake background wallpapers — only authentic physical dials floating over your desktop wallpaper.

---

## ✨ Key Features

- **Genuine Desktop Transparency:** Transparent per-pixel alpha window rendering. Desktop wallpaper remains completely visible around and between meters.
- **Independent Meter Movement:** Click and drag any gauge to place it anywhere on single or multi-monitor setups.
- **Independent Resizing:** Scroll mouse wheel directly over any gauge to scale it smoothly from 60% to 200%.
- **Position Persistence:** Coordinates are safely saved in `%APPDATA%\TorqueMonitor\settings.json` and restored on startup.
- **Damped Spring Physics (60 FPS):** Realistic needle acceleration, deceleration, and natural mechanical momentum.
- **Live Waveform History Graphs:** 40-point rolling history buffer rendering glowing vector activity traces inside each dial.
- **Multi-tiered Hardware Telemetry:** Powered by LibreHardwareMonitorLib, Windows Performance Counters, and Win32 APIs.
- **System Tray Integration:** Quick double-click restore, dark automotive context menu, and hardware sensor diagnostics.

---

## ⏱️ Six Primary Gauges

1. **CPU USAGE (0–100%):** Utilisation percentage, current clock speed (GHz), CPU model branding, and live activity graph.
2. **CPU TEMP (0–120°C):** Real core/package temperature, Blue (<50°C), Yellow (50–80°C), and Red (80–120°C) thermal zones.
3. **GPU USAGE (0–100%):** Dedicated GPU load percentage, core clock, GPU model branding, and rolling history graph.
4. **GPU TEMP (0–120°C):** GPU core temperature, hotspot telemetry, and thermal alert zones.
5. **RAM USAGE (0–100%):** Physical memory utilisation, used/total RAM in GB, and high-performance Emerald scale.
6. **DISK ACTIVITY (0–100%):** Storage active time percentage, independent Read and Write speeds (MB/s), and drive info.

---

## 🚀 Installation & Downloads

Pre-built self-contained binaries are available on the [GitHub Releases](https://github.com/tanymbarakah/torque-monitor/releases) page:

- **Standalone Portable:** `TorqueMonitor.exe` (Zero dependencies required).
- **Windows MSI Installer:** `TorqueMonitor-Setup-x64.msi` (Start menu & desktop shortcuts with clean uninstaller).

📘 For comprehensive documentation and operation details, please consult the [Official User Manual](USER_MANUAL.md).

---

## 🛡️ Windows SmartScreen & Security Information

### Why does Windows show "Windows protected your PC"?
When you download a new executable from the internet, Microsoft Defender SmartScreen displays an informational prompt because the software is brand-new and has not yet accumulated thousands of telemetry downloads in Microsoft's cloud reputation database.

### How to Run:
1. On the SmartScreen prompt, click **"More info"**.
2. Click **"Run anyway"**.
3. *Alternative (1-Click Trust):* Right-click `Install-Certificate.cmd` and select **Run as Administrator** to permanently add the NIHAR TECHNOLOGIES Root CA and Code Signing certificate to your trusted certificate store.

---

## 🔒 Binary Integrity & Authenticode Digital Signature

All distribution packages are digitally signed with Microsoft Authenticode and timestamped via DigiCert RFC 3161 authority.

- **Publisher / Signer:** `CN=Tanym Barakah, O=NIHAR TECHNOLOGIES, OU=Software Development, C=BD`
- **Issuer:** `CN=NIHAR TECHNOLOGIES Root CA, O=NIHAR TECHNOLOGIES, C=BD`
- **Code Signing Thumbprint:** `0D38A7932F2798E8549DF7CD569BCB7BD4DF9571`
- **Root CA Thumbprint:** `DD6130B9438DF95ED1EA494032219C3BAEF0E624`

| File | Format | SHA-256 Hash |
| :--- | :---: | :--- |
| **TorqueMonitor.exe** | Standalone x64 | `0C11F2AE6F4411368DBABB36178BDD728EFE75C888C8E7CDBE56650B7EB3B972` |
| **TorqueMonitor-Setup-x64.msi** | Windows MSI | `8CECAE11EA13FD4F533014AC8B19D6955BDE4590CC19C0BC8CE471AB4748796E` |

Verify digital signature in Windows:
Right-click file > **Properties** > **Digital Signatures** tab.

Verify hash in PowerShell:
```powershell
Get-FileHash -Algorithm SHA256 "TorqueMonitor.exe"
```

---

## 👨‍💻 Developer Information

- **Developer:** Tanym Barakah
- **Designation:** Founder & CEO @ NIHAR TECHNOLOGIES
- **Professional Title:** Software & App Developer
- **Organization:** [NIHAR TECHNOLOGIES](https://nihartechnologies.com)
- **Email:** [tanymbarakah@nihartechnologies.com](mailto:tanymbarakah@nihartechnologies.com)
- **Country:** Bangladesh

### Connect:
- **GitHub:** [@tanymbarakah](https://github.com/tanymbarakah)
- **LinkedIn:** [Tanym Barakah](https://www.linkedin.com/in/tanym-barakah/)
- **Facebook:** [tanymbarakah.me](https://www.facebook.com/tanymbarakah.me)

---

## 📄 License & Copyright

Copyright (C) 2026 **Tanym Barakah**. **NIHAR TECHNOLOGIES**. All Rights Reserved.

This software is released under a **Personal Use License (Non-Commercial Freeware)**. You are free to install and use this application for personal hardware monitoring. Commercial deployment, resale, or bundling without prior written authorization is strictly prohibited. See [LICENSE.txt](LICENSE.txt) for full terms.
