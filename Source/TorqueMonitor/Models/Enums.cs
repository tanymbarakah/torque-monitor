namespace TorqueMonitor.Models
{
    public enum GaugeType
    {
        CpuUsage,
        CpuTemp,
        GpuUsage,
        GpuTemp,
        RamUsage,
        DiskActivity,
        // Optional gauges
        NetworkActivity,
        CpuClock,
        GpuClock,
        SystemUptime
    }

    public enum GaugeSizePreset
    {
        Small,    // 190 px
        Standard, // 230 px
        Large,    // 270 px
        ExtraLarge // 320 px
    }
}
