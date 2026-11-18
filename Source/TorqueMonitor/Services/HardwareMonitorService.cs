using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using LibreHardwareMonitor.Hardware;
using TorqueMonitor.Models;

namespace TorqueMonitor.Services
{
    public class HardwareMonitorService : IHardwareMonitorService
    {
        private readonly object _syncLock = new();
        private Computer? _computer;
        private System.Threading.Timer? _pollTimer;
        private int _intervalMs = 1000;
        private bool _isDisposed;
        private bool _isLhmInitialized;
        private HardwareSnapshot _latestSnapshot = new();

        // Performance Counter Fallbacks
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _diskTimeCounter;
        private PerformanceCounter? _diskReadCounter;
        private PerformanceCounter? _diskWriteCounter;

        // Network tracking
        private long _lastNetworkBytes;
        private DateTime _lastNetworkTime = DateTime.UtcNow;

        public event EventHandler<HardwareSnapshot>? SnapshotUpdated;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public HardwareMonitorService()
        {
            InitializeCounters();
            InitializeLibreHardwareMonitor();

            // Run an immediate synchronous first poll
            PollHardware(null);
        }

        private void InitializeCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
                _ = _cpuCounter.NextValue();
            }
            catch
            {
                try
                {
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _ = _cpuCounter.NextValue();
                }
                catch
                {
                    _cpuCounter = null;
                }
            }

            try
            {
                _diskTimeCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
                _ = _diskTimeCounter.NextValue();
            }
            catch
            {
                _diskTimeCounter = null;
            }

            try
            {
                _diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
                _diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
                _ = _diskReadCounter.NextValue();
                _ = _diskWriteCounter.NextValue();
            }
            catch
            {
                _diskReadCounter = null;
                _diskWriteCounter = null;
            }
        }

        private void InitializeLibreHardwareMonitor()
        {
            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsStorageEnabled = true,
                    IsMotherboardEnabled = true,
                    IsNetworkEnabled = true
                };

                _computer.Open();
                _computer.Accept(new UpdateVisitor());
                _isLhmInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LibreHardwareMonitor initialization error: {ex.Message}");
                _isLhmInitialized = false;
            }
        }

        public void Start()
        {
            lock (_syncLock)
            {
                _pollTimer?.Dispose();
                _pollTimer = new System.Threading.Timer(PollHardware, null, 100, _intervalMs);
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
                _pollTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void SetInterval(int milliseconds)
        {
            _intervalMs = Math.Max(250, milliseconds);
            lock (_syncLock)
            {
                _pollTimer?.Change(0, _intervalMs);
            }
        }

        public HardwareSnapshot GetCurrentSnapshot()
        {
            lock (_syncLock)
            {
                return _latestSnapshot;
            }
        }

        private void PollHardware(object? state)
        {
            if (_isDisposed) return;

            var snapshot = new HardwareSnapshot
            {
                Timestamp = DateTime.UtcNow
            };

            // 1. Query LibreHardwareMonitor if available
            if (_isLhmInitialized && _computer != null)
            {
                try
                {
                    _computer.Accept(new UpdateVisitor());

                    // Prioritize dedicated GPU with sensors
                    var gpus = _computer.Hardware
                        .Where(h => h.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel)
                        .OrderByDescending(h => h.Sensors.Count())
                        .ToList();

                    foreach (var hw in _computer.Hardware)
                    {
                        if (hw.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel)
                        {
                            // Handled by prioritized list
                            continue;
                        }
                        ReadHardwareSensors(hw, snapshot);
                    }

                    if (gpus.Count > 0)
                    {
                        ReadHardwareSensors(gpus[0], snapshot);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHM Poll error: {ex.Message}");
                }
            }

            // 2. Query System Fallbacks for any missing sensors
            FillFallbacks(snapshot);

            lock (_syncLock)
            {
                _latestSnapshot = snapshot;
            }

            SnapshotUpdated?.Invoke(this, snapshot);
        }

        private void ReadHardwareSensors(IHardware hw, HardwareSnapshot snapshot)
        {
            switch (hw.HardwareType)
            {
                case HardwareType.Cpu:
                    if (string.IsNullOrEmpty(snapshot.CpuName) || snapshot.CpuName == "CPU")
                    {
                        snapshot.CpuName = CleanHardwareName(hw.Name);
                    }

                    foreach (var sensor in hw.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
                        {
                            snapshot.CpuUsagePercent = sensor.Value;
                        }
                        else if (sensor.SensorType == SensorType.Temperature)
                        {
                            if (sensor.Value.HasValue && sensor.Value.Value > 0)
                            {
                                if (sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase) ||
                                    sensor.Name.Contains("Tdie", StringComparison.OrdinalIgnoreCase) ||
                                    sensor.Name.Contains("Tctl", StringComparison.OrdinalIgnoreCase) ||
                                    sensor.Name.Contains("Core (Tctl/Tdie)", StringComparison.OrdinalIgnoreCase))
                                {
                                    snapshot.CpuTemperatureC = sensor.Value;
                                }
                                else if (snapshot.CpuTemperatureC == null && sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                                {
                                    snapshot.CpuTemperatureC = sensor.Value;
                                }
                            }
                        }
                        else if (sensor.SensorType == SensorType.Clock && (sensor.Name.Contains("Core #1", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Core #0", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Cores (Average)", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Max", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (sensor.Value.HasValue && sensor.Value.Value > 0)
                            {
                                snapshot.CpuClockSpeedGhz = Math.Round(sensor.Value.Value / 1000.0, 2);
                            }
                        }
                    }
                    break;

                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    if (string.IsNullOrEmpty(snapshot.GpuName) || snapshot.GpuName == "GPU")
                    {
                        snapshot.GpuName = CleanHardwareName(hw.Name);
                    }

                    foreach (var sensor in hw.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && (sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("D3D 3D", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) || !snapshot.GpuUsagePercent.HasValue)
                            {
                                snapshot.GpuUsagePercent = sensor.Value;
                            }
                        }
                        else if (sensor.SensorType == SensorType.Temperature)
                        {
                            if (sensor.Name.Contains("Hot Spot", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Hotspot", StringComparison.OrdinalIgnoreCase))
                            {
                                snapshot.GpuHotspotTemperatureC = sensor.Value;
                            }
                            else if (sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase))
                            {
                                snapshot.GpuTemperatureC = sensor.Value;
                            }
                        }
                        else if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                        {
                            if (sensor.Value.HasValue && sensor.Value.Value > 0)
                            {
                                snapshot.GpuClockSpeedGhz = Math.Round(sensor.Value.Value / 1000.0, 2);
                            }
                        }
                    }
                    break;

                case HardwareType.Memory:
                    // Only match "Total Memory" or if not set
                    if (hw.Name.Contains("Total", StringComparison.OrdinalIgnoreCase) || !snapshot.RamUsagePercent.HasValue)
                    {
                        foreach (var sensor in hw.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase))
                            {
                                snapshot.RamUsagePercent = sensor.Value;
                            }
                            else if (sensor.SensorType == SensorType.Data)
                            {
                                if (sensor.Name.Contains("Used", StringComparison.OrdinalIgnoreCase))
                                    snapshot.RamUsedGb = Math.Round(sensor.Value ?? 0, 1);
                                else if (sensor.Name.Contains("Available", StringComparison.OrdinalIgnoreCase))
                                    snapshot.RamAvailableGb = Math.Round(sensor.Value ?? 0, 1);
                            }
                        }
                        if (snapshot.RamUsedGb.HasValue && snapshot.RamAvailableGb.HasValue)
                        {
                            snapshot.RamTotalGb = Math.Round(snapshot.RamUsedGb.Value + snapshot.RamAvailableGb.Value, 1);
                        }
                    }
                    break;

                case HardwareType.Storage:
                    if (string.IsNullOrEmpty(snapshot.DiskName) || snapshot.DiskName == "Primary Drive")
                    {
                        snapshot.DiskName = CleanHardwareName(hw.Name);
                    }
                    foreach (var sensor in hw.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && (sensor.Name.Contains("Total Activity", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Used Space", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Activity", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (sensor.Name.Contains("Activity", StringComparison.OrdinalIgnoreCase))
                            {
                                snapshot.DiskActiveTimePercent = sensor.Value;
                            }
                        }
                        else if (sensor.SensorType == SensorType.Throughput)
                        {
                            if (sensor.Name.Contains("Read Rate", StringComparison.OrdinalIgnoreCase) && sensor.Value.HasValue)
                            {
                                snapshot.DiskReadSpeedMbPerSec = Math.Round(sensor.Value.Value / (1024.0 * 1024.0), 1);
                            }
                            else if (sensor.Name.Contains("Write Rate", StringComparison.OrdinalIgnoreCase) && sensor.Value.HasValue)
                            {
                                snapshot.DiskWriteSpeedMbPerSec = Math.Round(sensor.Value.Value / (1024.0 * 1024.0), 1);
                            }
                        }
                    }
                    break;
            }

            foreach (var sub in hw.SubHardware)
            {
                ReadHardwareSensors(sub, snapshot);
            }
        }

        private void FillFallbacks(HardwareSnapshot snapshot)
        {
            // CPU fallback
            if (!snapshot.CpuUsagePercent.HasValue && _cpuCounter != null)
            {
                try
                {
                    snapshot.CpuUsagePercent = Math.Round(_cpuCounter.NextValue(), 1);
                }
                catch { }
            }

            // CPU Clock fallback via WMI if needed
            if (!snapshot.CpuClockSpeedGhz.HasValue)
            {
                try
                {
                    using var searcher = new System.Management.ManagementObjectSearcher("SELECT CurrentClockSpeed, Name FROM Win32_Processor");
                    foreach (var obj in searcher.Get())
                    {
                        if (obj["CurrentClockSpeed"] != null)
                        {
                            uint speedMhz = (uint)obj["CurrentClockSpeed"];
                            snapshot.CpuClockSpeedGhz = Math.Round(speedMhz / 1000.0, 2);
                        }
                        if ((string.IsNullOrEmpty(snapshot.CpuName) || snapshot.CpuName == "CPU") && obj["Name"] != null)
                        {
                            snapshot.CpuName = CleanHardwareName(obj["Name"]?.ToString() ?? "CPU");
                        }
                        break;
                    }
                }
                catch { }
            }

            // RAM fallback (using Win32 GlobalMemoryStatusEx - guaranteed 100% accurate without admin)
            if (!snapshot.RamUsagePercent.HasValue || !snapshot.RamUsedGb.HasValue)
            {
                try
                {
                    var memStatus = new MEMORYSTATUSEX();
                    if (GlobalMemoryStatusEx(memStatus))
                    {
                        double totalGb = Math.Round(memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0), 1);
                        double availGb = Math.Round(memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0), 1);
                        double usedGb = Math.Round(totalGb - availGb, 1);
                        snapshot.RamTotalGb = totalGb;
                        snapshot.RamAvailableGb = availGb;
                        snapshot.RamUsedGb = usedGb;
                        snapshot.RamUsagePercent = (double)memStatus.dwMemoryLoad;
                    }
                }
                catch { }
            }

            // Disk fallback
            if (!snapshot.DiskActiveTimePercent.HasValue && _diskTimeCounter != null)
            {
                try
                {
                    float diskTime = _diskTimeCounter.NextValue();
                    snapshot.DiskActiveTimePercent = Math.Clamp(Math.Round(diskTime, 1), 0, 100);
                }
                catch { }
            }

            if (!snapshot.DiskReadSpeedMbPerSec.HasValue && _diskReadCounter != null)
            {
                try
                {
                    float bytesPerSec = _diskReadCounter.NextValue();
                    snapshot.DiskReadSpeedMbPerSec = Math.Round(bytesPerSec / (1024.0 * 1024.0), 1);
                }
                catch { }
            }

            if (!snapshot.DiskWriteSpeedMbPerSec.HasValue && _diskWriteCounter != null)
            {
                try
                {
                    float bytesPerSec = _diskWriteCounter.NextValue();
                    snapshot.DiskWriteSpeedMbPerSec = Math.Round(bytesPerSec / (1024.0 * 1024.0), 1);
                }
                catch { }
            }

            // Network fallback
            try
            {
                long currentBytes = 0;
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var stats = nic.GetIPv4Statistics();
                        currentBytes += stats.BytesReceived + stats.BytesSent;
                    }
                }

                var now = DateTime.UtcNow;
                double seconds = (now - _lastNetworkTime).TotalSeconds;
                if (seconds > 0 && _lastNetworkBytes > 0)
                {
                    long diff = currentBytes - _lastNetworkBytes;
                    if (diff >= 0)
                    {
                        double mbps = (diff * 8.0) / (seconds * 1024.0 * 1024.0);
                        snapshot.NetworkSpeedMbps = Math.Round(mbps, 2);
                    }
                }
                _lastNetworkBytes = currentBytes;
                _lastNetworkTime = now;
            }
            catch { }

            // System Uptime
            try
            {
                snapshot.SystemUptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            }
            catch { }
        }

        private static string CleanHardwareName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Hardware";
            return raw.Replace("Microsoft", "")
                      .Replace("Corporation", "")
                      .Replace("Processor", "")
                      .Replace("Graphics", "")
                      .Replace("(TM)", "")
                      .Replace("(R)", "")
                      .Trim();
        }

        public string GetHardwareDiagnostics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TORQUE MONITOR HARDWARE DIAGNOSTICS ===");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"OS Version: {Environment.OSVersion}");
            sb.AppendLine($"64-Bit OS: {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"Processor Count: {Environment.ProcessorCount}");
            sb.AppendLine($"LibreHardwareMonitor Initialized: {_isLhmInitialized}");
            sb.AppendLine();

            if (_isLhmInitialized && _computer != null)
            {
                sb.AppendLine("--- DETECTED HARDWARE & SENSORS ---");
                foreach (var hw in _computer.Hardware)
                {
                    sb.AppendLine($"Hardware: {hw.Name} [{hw.HardwareType}]");
                    foreach (var s in hw.Sensors)
                    {
                        sb.AppendLine($"   - Sensor: {s.Name} ({s.SensorType}) = {s.Value}");
                    }
                }
            }
            else
            {
                sb.AppendLine("LibreHardwareMonitor not active. Performance Counters and Win32 fallback in use.");
            }

            var snap = GetCurrentSnapshot();
            sb.AppendLine();
            sb.AppendLine("--- CURRENT SNAPSHOT ---");
            sb.AppendLine($"CPU: {snap.CpuName} | Load: {snap.CpuUsagePercent?.ToString("F1") ?? "N/A"}% | Temp: {snap.CpuTemperatureC?.ToString("F1") ?? "N/A"}°C | Clock: {snap.CpuClockSpeedGhz?.ToString("F2") ?? "N/A"} GHz");
            sb.AppendLine($"GPU: {snap.GpuName} | Load: {snap.GpuUsagePercent?.ToString("F1") ?? "N/A"}% | Temp: {snap.GpuTemperatureC?.ToString("F1") ?? "N/A"}°C | Hotspot: {snap.GpuHotspotTemperatureC?.ToString("F1") ?? "N/A"}°C");
            sb.AppendLine($"RAM: {snap.RamUsedGb?.ToString("F1") ?? "N/A"} / {snap.RamTotalGb?.ToString("F1") ?? "N/A"} GB ({snap.RamUsagePercent?.ToString("F0") ?? "N/A"}%)");
            sb.AppendLine($"DISK: {snap.DiskName} | Active: {snap.DiskActiveTimePercent?.ToString("F1") ?? "N/A"}% | Read: {snap.DiskReadSpeedMbPerSec?.ToString("F1") ?? "N/A"} MB/s | Write: {snap.DiskWriteSpeedMbPerSec?.ToString("F1") ?? "N/A"} MB/s");

            return sb.ToString();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _pollTimer?.Dispose();
            _cpuCounter?.Dispose();
            _diskTimeCounter?.Dispose();
            _diskReadCounter?.Dispose();
            _diskWriteCounter?.Dispose();

            if (_computer != null)
            {
                try
                {
                    _computer.Close();
                }
                catch { }
            }
        }

        private class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (var sub in hardware.SubHardware)
                {
                    sub.Accept(this);
                }
            }

            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
    }
}
