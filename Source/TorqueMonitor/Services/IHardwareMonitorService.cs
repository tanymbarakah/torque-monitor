using TorqueMonitor.Models;

namespace TorqueMonitor.Services
{
    public interface IHardwareMonitorService : IDisposable
    {
        event EventHandler<HardwareSnapshot>? SnapshotUpdated;
        HardwareSnapshot GetCurrentSnapshot();
        void Start();
        void Stop();
        void SetInterval(int milliseconds);
        string GetHardwareDiagnostics();
    }
}
