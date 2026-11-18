using System.Threading;
using System.Windows;
using TorqueMonitor.Services;
using TorqueMonitor.Views;

namespace TorqueMonitor
{
    public partial class App : Application
    {
        private static Mutex? _singleInstanceMutex;
        private WindowManager? _windowManager;
        private TrayIconService? _trayIconService;
        private HardwareMonitorService? _hardwareService;
        private SettingsService? _settingsService;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Ensure single instance
            const string appGuid = "TorqueMonitor-8A4E1B32-9F2D-4B1C-B738-92A385E8C940";
            _singleInstanceMutex = new Mutex(true, appGuid, out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show("TORQUE MONITOR is already running in the background. Check your system tray.", "Torque Monitor", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            base.OnStartup(e);

            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled domain exception: {args.ExceptionObject}");
            };
            DispatcherUnhandledException += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled dispatcher exception: {args.Exception.Message}");
                args.Handled = true; // prevent application crash
            };

            // Initialize services
            _settingsService = new SettingsService();
            _hardwareService = new HardwareMonitorService();

            // Set configured refresh interval
            _hardwareService.SetInterval(_settingsService.Current.RefreshIntervalMs);
            _hardwareService.Start();

            // Initialize window and tray managers
            _windowManager = new WindowManager(_settingsService, _hardwareService);
            _trayIconService = new TrayIconService(_windowManager);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIconService?.Dispose();
            _windowManager?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
            base.OnExit(e);
        }
    }
}
