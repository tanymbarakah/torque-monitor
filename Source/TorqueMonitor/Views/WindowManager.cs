using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using TorqueMonitor.Models;
using TorqueMonitor.Services;
using TorqueMonitor.ViewModels;

namespace TorqueMonitor.Views
{
    public class WindowManager : IDisposable
    {
        private readonly List<GaugeWindow> _gaugeWindows = new();
        private readonly Dictionary<GaugeType, GaugeViewModel> _viewModels = new();
        private readonly SettingsService _settingsService;
        private readonly IHardwareMonitorService _hardwareService;
        private long _lastRenderTicks;
        private bool _isDisposed;

        public SettingsService SettingsService => _settingsService;
        public IHardwareMonitorService HardwareService => _hardwareService;
        public IReadOnlyList<GaugeWindow> AllGaugeWindows => _gaugeWindows;

        public WindowManager(SettingsService settingsService, IHardwareMonitorService hardwareService)
        {
            _settingsService = settingsService;
            _hardwareService = hardwareService;

            InitializeGauges();

            // Connect hardware update events
            _hardwareService.SnapshotUpdated += OnHardwareSnapshotUpdated;

            // 60 FPS Smooth needle physics animation loop
            _lastRenderTicks = Stopwatch.GetTimestamp();
            CompositionTarget.Rendering += OnCompositionRendering;
        }

        private void InitializeGauges()
        {
            var gaugeDefinitions = CreateDefaultGaugeData();

            foreach (var def in gaugeDefinitions)
            {
                var vm = new GaugeViewModel(def);
                _viewModels[def.Type] = vm;

                var settings = _settingsService.Current.Gauges.FirstOrDefault(g => g.Type == def.Type);
                if (settings == null)
                {
                    settings = new GaugeSettings
                    {
                        Type = def.Type,
                        IsVisible = IsPrimaryGauge(def.Type)
                    };
                    _settingsService.Current.Gauges.Add(settings);
                }

                // Multi-monitor boundary safety clamp
                _settingsService.ClampGaugePosition(settings, 240 * settings.Scale);

                var window = new GaugeWindow(vm, settings, this);
                _gaugeWindows.Add(window);

                if (settings.IsVisible)
                {
                    window.Show();
                }
            }
        }

        private static bool IsPrimaryGauge(GaugeType type)
        {
            return type switch
            {
                GaugeType.CpuUsage => true,
                GaugeType.CpuTemp => true,
                GaugeType.GpuUsage => true,
                GaugeType.GpuTemp => true,
                GaugeType.RamUsage => true,
                GaugeType.DiskActivity => true,
                _ => false
            };
        }

        private static List<GaugeData> CreateDefaultGaugeData()
        {
            return new List<GaugeData>
            {
                // GAUGE 01 — CPU USAGE (0–100%)
                new()
                {
                    Type = GaugeType.CpuUsage,
                    Title = "CPU",
                    MinValue = 0,
                    MaxValue = 100,
                    Unit = "%",
                    AccentColor = Color.FromRgb(0, 229, 255), // Cyan
                    GlowColor = Color.FromArgb(140, 0, 229, 255),
                    HasZones = false
                },

                // GAUGE 02 — CPU TEMP (0–120°C)
                new()
                {
                    Type = GaugeType.CpuTemp,
                    Title = "CPU TEMP",
                    MinValue = 0,
                    MaxValue = 120,
                    Unit = "°C",
                    AccentColor = Color.FromRgb(255, 42, 66), // Red
                    GlowColor = Color.FromArgb(140, 255, 42, 66),
                    HasZones = true,
                    LowZoneColor = Color.FromRgb(0, 168, 255),   // Blue <50
                    MidZoneColor = Color.FromRgb(255, 193, 7),   // Yellow 50-80
                    HighZoneColor = Color.FromRgb(255, 42, 66)   // Red 80-120
                },

                // GAUGE 03 — GPU USAGE (0–100%)
                new()
                {
                    Type = GaugeType.GpuUsage,
                    Title = "GPU",
                    MinValue = 0,
                    MaxValue = 100,
                    Unit = "%",
                    AccentColor = Color.FromRgb(0, 210, 255), // Vivid Sapphire / Cyan
                    GlowColor = Color.FromArgb(140, 0, 210, 255),
                    HasZones = false
                },

                // GAUGE 04 — GPU TEMP (0–120°C)
                new()
                {
                    Type = GaugeType.GpuTemp,
                    Title = "GPU TEMP",
                    MinValue = 0,
                    MaxValue = 120,
                    Unit = "°C",
                    AccentColor = Color.FromRgb(255, 75, 75),
                    GlowColor = Color.FromArgb(140, 255, 75, 75),
                    HasZones = true,
                    LowZoneColor = Color.FromRgb(0, 168, 255),
                    MidZoneColor = Color.FromRgb(255, 193, 7),
                    HighZoneColor = Color.FromRgb(255, 42, 66)
                },

                // GAUGE 05 — RAM USAGE (0–100%)
                new()
                {
                    Type = GaugeType.RamUsage,
                    Title = "RAM",
                    MinValue = 0,
                    MaxValue = 100,
                    Unit = "%",
                    AccentColor = Color.FromRgb(0, 230, 118), // Performance Emerald / Mint
                    GlowColor = Color.FromArgb(140, 0, 230, 118),
                    HasZones = false
                },

                // GAUGE 06 — DISK ACTIVITY (0–100%)
                new()
                {
                    Type = GaugeType.DiskActivity,
                    Title = "DISK",
                    MinValue = 0,
                    MaxValue = 100,
                    Unit = "%",
                    AccentColor = Color.FromRgb(255, 179, 0), // Amber
                    GlowColor = Color.FromArgb(140, 255, 179, 0),
                    HasZones = false
                },

                // Optional Gauges
                new()
                {
                    Type = GaugeType.NetworkActivity,
                    Title = "NETWORK",
                    MinValue = 0,
                    MaxValue = 100,
                    Unit = "Mbps",
                    AccentColor = Color.FromRgb(168, 85, 247), // Purple / Violet
                    HasZones = false
                },
                new()
                {
                    Type = GaugeType.CpuClock,
                    Title = "CPU CLOCK",
                    MinValue = 0,
                    MaxValue = 6,
                    Unit = "GHz",
                    AccentColor = Color.FromRgb(56, 189, 248),
                    HasZones = false
                },
                new()
                {
                    Type = GaugeType.GpuClock,
                    Title = "GPU CLOCK",
                    MinValue = 0,
                    MaxValue = 3,
                    Unit = "GHz",
                    AccentColor = Color.FromRgb(244, 63, 94),
                    HasZones = false
                },
                new()
                {
                    Type = GaugeType.SystemUptime,
                    Title = "UPTIME",
                    MinValue = 0,
                    MaxValue = 24,
                    Unit = "Hrs",
                    AccentColor = Color.FromRgb(14, 165, 233),
                    HasZones = false
                }
            };
        }

        private void OnHardwareSnapshotUpdated(object? sender, HardwareSnapshot snapshot)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // 1. CPU Usage
                if (_viewModels.TryGetValue(GaugeType.CpuUsage, out var cpuUsageVm))
                {
                    string clockStr = snapshot.CpuClockSpeedGhz.HasValue ? $"{snapshot.CpuClockSpeedGhz.Value:F2} GHz" : "";
                    cpuUsageVm.UpdateTelemetry(snapshot.CpuUsagePercent, clockStr, snapshot.CpuName, snapshot.CpuUsagePercent.HasValue);
                }

                // 2. CPU Temp
                if (_viewModels.TryGetValue(GaugeType.CpuTemp, out var cpuTempVm))
                {
                    cpuTempVm.UpdateTelemetry(snapshot.CpuTemperatureC, "CPU PACKAGE", snapshot.CpuName, snapshot.CpuTemperatureC.HasValue);
                }

                // 3. GPU Usage
                if (_viewModels.TryGetValue(GaugeType.GpuUsage, out var gpuUsageVm))
                {
                    string gpuClockStr = snapshot.GpuClockSpeedGhz.HasValue ? $"{snapshot.GpuClockSpeedGhz.Value:F2} GHz" : "";
                    gpuUsageVm.UpdateTelemetry(snapshot.GpuUsagePercent, gpuClockStr, snapshot.GpuName, snapshot.GpuUsagePercent.HasValue);
                }

                // 4. GPU Temp
                if (_viewModels.TryGetValue(GaugeType.GpuTemp, out var gpuTempVm))
                {
                    string hotspotStr = snapshot.GpuHotspotTemperatureC.HasValue ? $"Hotspot: {snapshot.GpuHotspotTemperatureC.Value:F0}°C" : "GPU CORE";
                    gpuTempVm.UpdateTelemetry(snapshot.GpuTemperatureC, hotspotStr, snapshot.GpuName, snapshot.GpuTemperatureC.HasValue);
                }

                // 5. RAM Usage
                if (_viewModels.TryGetValue(GaugeType.RamUsage, out var ramUsageVm))
                {
                    string ramStr = (snapshot.RamUsedGb.HasValue && snapshot.RamTotalGb.HasValue)
                        ? $"{snapshot.RamUsedGb.Value:F1} / {snapshot.RamTotalGb.Value:F1} GB"
                        : "";
                    ramUsageVm.UpdateTelemetry(snapshot.RamUsagePercent, ramStr, "System Memory", snapshot.RamUsagePercent.HasValue);
                }

                // 6. Disk Activity
                if (_viewModels.TryGetValue(GaugeType.DiskActivity, out var diskVm))
                {
                    string rwStr = (snapshot.DiskReadSpeedMbPerSec.HasValue || snapshot.DiskWriteSpeedMbPerSec.HasValue)
                        ? $"R: {snapshot.DiskReadSpeedMbPerSec ?? 0:F0} | W: {snapshot.DiskWriteSpeedMbPerSec ?? 0:F0} MB/s"
                        : "Active Time";
                    diskVm.UpdateTelemetry(snapshot.DiskActiveTimePercent, rwStr, snapshot.DiskName, snapshot.DiskActiveTimePercent.HasValue);
                }

                // Optional: Network
                if (_viewModels.TryGetValue(GaugeType.NetworkActivity, out var netVm))
                {
                    string netSpeed = snapshot.NetworkSpeedMbps.HasValue ? $"{snapshot.NetworkSpeedMbps.Value:F1} Mbps" : "N/A";
                    netVm.UpdateTelemetry(snapshot.NetworkSpeedMbps, netSpeed, "Adapter", snapshot.NetworkSpeedMbps.HasValue);
                }

                // Optional: CPU Clock
                if (_viewModels.TryGetValue(GaugeType.CpuClock, out var cpuClockVm))
                {
                    cpuClockVm.UpdateTelemetry(snapshot.CpuClockSpeedGhz, snapshot.CpuName, "Clock Speed", snapshot.CpuClockSpeedGhz.HasValue);
                }

                // Optional: GPU Clock
                if (_viewModels.TryGetValue(GaugeType.GpuClock, out var gpuClockVm))
                {
                    gpuClockVm.UpdateTelemetry(snapshot.GpuClockSpeedGhz, snapshot.GpuName, "Core Clock", snapshot.GpuClockSpeedGhz.HasValue);
                }

                // Optional: System Uptime
                if (_viewModels.TryGetValue(GaugeType.SystemUptime, out var uptimeVm))
                {
                    if (snapshot.SystemUptime.HasValue)
                    {
                        var ts = snapshot.SystemUptime.Value;
                        string uptimeStr = $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";
                        uptimeVm.UpdateTelemetry(ts.TotalHours, uptimeStr, "Windows", true);
                    }
                }
            });
        }

        private void OnCompositionRendering(object? sender, EventArgs e)
        {
            long currentTicks = Stopwatch.GetTimestamp();
            double dt = (double)(currentTicks - _lastRenderTicks) / Stopwatch.Frequency;
            _lastRenderTicks = currentTicks;

            // Clamp dt to prevent frame lag spikes
            if (dt > 0.1) dt = 0.1;
            if (dt <= 0) return;

            foreach (var vm in _viewModels.Values)
            {
                vm.AnimatePhysics(dt);
            }
        }

        public void ResetToDefaultGridLayout()
        {
            // Position 3 columns x 2 rows cleanly on primary screen
            double primaryWidth = SystemParameters.PrimaryScreenWidth;
            double primaryHeight = SystemParameters.PrimaryScreenHeight;

            const double gaugeSize = 240.0;
            const double spacingX = 30.0;
            const double spacingY = 30.0;

            double totalGridWidth = (3 * gaugeSize) + (2 * spacingX);
            double totalGridHeight = (2 * gaugeSize) + (1 * spacingY);

            double startX = Math.Max(40, (primaryWidth - totalGridWidth) / 2.0);
            double startY = Math.Max(40, (primaryHeight - totalGridHeight) / 2.0);

            var primaryLayout = new[]
            {
                // Top Row: 1. CPU Usage, 2. CPU Temp, 3. GPU Usage
                new { Type = GaugeType.CpuUsage, Col = 0, Row = 0 },
                new { Type = GaugeType.CpuTemp, Col = 1, Row = 0 },
                new { Type = GaugeType.GpuUsage, Col = 2, Row = 0 },
                // Bottom Row: 4. GPU Temp, 5. RAM Usage, 6. Disk Activity
                new { Type = GaugeType.GpuTemp, Col = 0, Row = 1 },
                new { Type = GaugeType.RamUsage, Col = 1, Row = 1 },
                new { Type = GaugeType.DiskActivity, Col = 2, Row = 1 }
            };

            foreach (var item in primaryLayout)
            {
                var gw = _gaugeWindows.FirstOrDefault(g => g.GaugeType == item.Type);
                if (gw != null)
                {
                    double targetX = startX + (item.Col * (gaugeSize + spacingX));
                    double targetY = startY + (item.Row * (gaugeSize + spacingY));

                    gw.Left = targetX;
                    gw.Top = targetY;
                    gw.ApplyScale(1.0);
                    gw.Visibility = Visibility.Visible;

                    var gSettings = _settingsService.Current.Gauges.FirstOrDefault(g => g.Type == item.Type);
                    if (gSettings != null)
                    {
                        gSettings.X = targetX;
                        gSettings.Y = targetY;
                        gSettings.Scale = 1.0;
                        gSettings.IsVisible = true;
                    }
                }
            }

            SaveSettings();
        }

        public void ShowAllPrimaryGauges()
        {
            foreach (var gw in _gaugeWindows)
            {
                if (IsPrimaryGauge(gw.GaugeType))
                {
                    gw.Visibility = Visibility.Visible;
                    var s = _settingsService.Current.Gauges.FirstOrDefault(g => g.Type == gw.GaugeType);
                    if (s != null) s.IsVisible = true;
                }
            }
            SaveSettings();
        }

        public void HideAllGauges()
        {
            foreach (var gw in _gaugeWindows)
            {
                gw.Visibility = Visibility.Collapsed;
                var s = _settingsService.Current.Gauges.FirstOrDefault(g => g.Type == gw.GaugeType);
                if (s != null) s.IsVisible = false;
            }
            SaveSettings();
        }

        public void SetGlobalOpacity(double opacity)
        {
            _settingsService.Current.GlobalOpacity = opacity;
            foreach (var gw in _gaugeWindows)
            {
                gw.Opacity = opacity;
            }
            SaveSettings();
        }

        public void SetAllGaugesScale(double scale)
        {
            foreach (var gw in _gaugeWindows)
            {
                gw.ApplyScale(scale);
            }
            SaveSettings();
        }

        public void SetGlobalLockPositions(bool locked)
        {
            _settingsService.Current.GlobalLockPositions = locked;
            foreach (var gw in _gaugeWindows)
            {
                gw.ApplySettings();
            }
            SaveSettings();
        }

        public void SetAlwaysOnTop(bool topmost)
        {
            _settingsService.Current.AlwaysOnTop = topmost;
            foreach (var gw in _gaugeWindows)
            {
                gw.Topmost = topmost;
            }
            SaveSettings();
        }

        public void SetRefreshInterval(int ms)
        {
            _settingsService.Current.RefreshIntervalMs = ms;
            _hardwareService.SetInterval(ms);
            SaveSettings();
        }

        public void SetStartWithWindows(bool enable)
        {
            _settingsService.SetStartWithWindows(enable);
        }

        public void ShowHardwareDiagnostics()
        {
            string diagText = _hardwareService.GetHardwareDiagnostics();
            var dlg = new HardwareInfoDialog(diagText);
            dlg.Show();
        }

        public void ShowDeveloperInfo()
        {
            var dlg = new DeveloperInfoDialog();
            dlg.Show();
        }

        public void SaveSettings()
        {
            foreach (var gw in _gaugeWindows)
            {
                var s = _settingsService.Current.Gauges.FirstOrDefault(g => g.Type == gw.GaugeType);
                if (s != null)
                {
                    s.X = gw.Left;
                    s.Y = gw.Top;
                }
            }
            _settingsService.SaveSettings();
        }

        public void ShutdownApplication()
        {
            SaveSettings();
            Dispose();
            Application.Current.Shutdown();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            CompositionTarget.Rendering -= OnCompositionRendering;
            _hardwareService.SnapshotUpdated -= OnHardwareSnapshotUpdated;
            _hardwareService.Dispose();

            foreach (var gw in _gaugeWindows)
            {
                gw.Close();
            }
        }
    }
}
