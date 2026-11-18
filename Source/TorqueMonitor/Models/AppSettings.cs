namespace TorqueMonitor.Models
{
    public class GaugeSettings
    {
        public GaugeType Type { get; set; }
        public double X { get; set; } = 100;
        public double Y { get; set; } = 100;
        public double Scale { get; set; } = 1.0;
        public bool IsVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;
    }

    public class AppSettings
    {
        public int RefreshIntervalMs { get; set; } = 1000;
        public bool AlwaysOnTop { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;
        public double GlobalOpacity { get; set; } = 1.0;
        public bool GlobalLockPositions { get; set; } = false;
        public GaugeSizePreset DefaultSizePreset { get; set; } = GaugeSizePreset.Standard;
        public List<GaugeSettings> Gauges { get; set; } = new();

        public static AppSettings CreateDefault()
        {
            var settings = new AppSettings();
            // Default 3x2 grid layout (computed dynamically relative to screen, but good defaults)
            settings.Gauges = new List<GaugeSettings>
            {
                new() { Type = GaugeType.CpuUsage, X = 80, Y = 100, Scale = 1.0, IsVisible = true },
                new() { Type = GaugeType.CpuTemp, X = 330, Y = 100, Scale = 1.0, IsVisible = true },
                new() { Type = GaugeType.GpuUsage, X = 580, Y = 100, Scale = 1.0, IsVisible = true },
                new() { Type = GaugeType.GpuTemp, X = 80, Y = 350, Scale = 1.0, IsVisible = true },
                new() { Type = GaugeType.RamUsage, X = 330, Y = 350, Scale = 1.0, IsVisible = true },
                new() { Type = GaugeType.DiskActivity, X = 580, Y = 350, Scale = 1.0, IsVisible = true },

                // Optional gauges (hidden by default)
                new() { Type = GaugeType.NetworkActivity, X = 830, Y = 100, Scale = 1.0, IsVisible = false },
                new() { Type = GaugeType.CpuClock, X = 830, Y = 350, Scale = 1.0, IsVisible = false },
                new() { Type = GaugeType.GpuClock, X = 1080, Y = 100, Scale = 1.0, IsVisible = false },
                new() { Type = GaugeType.SystemUptime, X = 1080, Y = 350, Scale = 1.0, IsVisible = false }
            };
            return settings;
        }
    }
}
