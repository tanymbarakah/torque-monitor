using System.Windows.Media;

namespace TorqueMonitor.Models
{
    public class GaugeData
    {
        public GaugeType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public double Value { get; set; }
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 100;
        public string FormattedValue { get; set; } = "0";
        public string Unit { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ExtraInfo { get; set; } = string.Empty;
        public bool IsSensorAvailable { get; set; } = true;

        // Accent / Lighting Colors
        public Color AccentColor { get; set; } = Color.FromRgb(0, 229, 255); // Default Cyan
        public Color GlowColor { get; set; } = Color.FromArgb(120, 0, 229, 255);

        // Scale Zones
        public bool HasZones { get; set; } = false;
        public double WarningThreshold { get; set; } = 70;
        public double DangerThreshold { get; set; } = 85;

        // Colors for Zones
        public Color LowZoneColor { get; set; } = Color.FromRgb(0, 168, 255);   // Blue
        public Color MidZoneColor { get; set; } = Color.FromRgb(255, 193, 7);   // Yellow/Amber
        public Color HighZoneColor { get; set; } = Color.FromRgb(255, 42, 66);   // Red
    }

    public class HardwareSnapshot
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // CPU
        public double? CpuUsagePercent { get; set; }
        public double? CpuTemperatureC { get; set; }
        public double? CpuClockSpeedGhz { get; set; }
        public string CpuName { get; set; } = "CPU";

        // GPU
        public double? GpuUsagePercent { get; set; }
        public double? GpuTemperatureC { get; set; }
        public double? GpuHotspotTemperatureC { get; set; }
        public double? GpuClockSpeedGhz { get; set; }
        public string GpuName { get; set; } = "GPU";

        // RAM
        public double? RamUsagePercent { get; set; }
        public double? RamUsedGb { get; set; }
        public double? RamTotalGb { get; set; }
        public double? RamAvailableGb { get; set; }

        // Disk
        public double? DiskActiveTimePercent { get; set; }
        public double? DiskReadSpeedMbPerSec { get; set; }
        public double? DiskWriteSpeedMbPerSec { get; set; }
        public string DiskName { get; set; } = "Primary Drive";

        // Optional Metrics
        public double? NetworkSpeedMbps { get; set; }
        public TimeSpan? SystemUptime { get; set; }
    }
}
