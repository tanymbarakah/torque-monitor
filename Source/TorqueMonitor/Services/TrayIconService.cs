using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using TorqueMonitor.Views;

namespace TorqueMonitor.Services
{
    public class TrayIconService : IDisposable
    {
        private const int WM_USER = 0x0400;
        private const int WM_TRAYICON = WM_USER + 1024;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;

        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;

        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;
        private const uint LR_DEFAULTSIZE = 0x00000040;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(int dwMessage, [In] ref NOTIFYICONDATA lpData);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private readonly WindowManager _windowManager;
        private HwndSource? _hwndSource;
        private IntPtr _hIcon = IntPtr.Zero;
        private bool _isDisposed;
        private ContextMenu? _trayContextMenu;

        public TrayIconService(WindowManager windowManager)
        {
            _windowManager = windowManager;
            InitializeMessageWindow();
            CreateTrayIcon();
        }

        private void InitializeMessageWindow()
        {
            var parameters = new HwndSourceParameters("TorqueMonitorTrayMsgWindow")
            {
                WindowStyle = 0,
                Width = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(WndProc);
        }

        private void CreateTrayIcon()
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string iconPath = Path.Combine(exeDir, "Resources", "AppIcon.ico");

                if (File.Exists(iconPath))
                {
                    _hIcon = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
                }

                if (_hIcon == IntPtr.Zero)
                {
                    string? exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                    {
                        ExtractIconEx(exePath, 0, out _, out _hIcon, 1);
                    }
                }

                if (_hwndSource == null) return;

                var data = new NOTIFYICONDATA
                {
                    cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                    hWnd = _hwndSource.Handle,
                    uID = 1001,
                    uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                    uCallbackMessage = WM_TRAYICON,
                    hIcon = _hIcon,
                    szTip = "TORQUE MONITOR — Automotive Telemetry"
                };

                Shell_NotifyIcon(NIM_ADD, ref data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to add tray icon: {ex.Message}");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_TRAYICON)
            {
                int mouseEvent = lParam.ToInt32();
                if (mouseEvent == WM_RBUTTONUP)
                {
                    ShowTrayMenu();
                    handled = true;
                }
                else if (mouseEvent == WM_LBUTTONDBLCLK)
                {
                    _windowManager.ShowAllPrimaryGauges();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void ShowTrayMenu()
        {
            if (_hwndSource == null) return;

            SetForegroundWindow(_hwndSource.Handle);

            _trayContextMenu = new ContextMenu();

            // Header
            var titleItem = new MenuItem
            {
                Header = "TORQUE MONITOR v1.0",
                IsEnabled = false,
                FontWeight = FontWeights.Bold
            };
            _trayContextMenu.Items.Add(titleItem);
            _trayContextMenu.Items.Add(new Separator());

            // Show All
            var showAll = new MenuItem { Header = "Show All 6 Gauges" };
            showAll.Click += (s, e) => _windowManager.ShowAllPrimaryGauges();
            _trayContextMenu.Items.Add(showAll);

            // Hide All
            var hideAll = new MenuItem { Header = "Hide All Gauges" };
            hideAll.Click += (s, e) => _windowManager.HideAllGauges();
            _trayContextMenu.Items.Add(hideAll);

            // Reset 3x2 Layout
            var resetLayout = new MenuItem { Header = "Reset to 3x2 Grid Layout" };
            resetLayout.Click += (s, e) => _windowManager.ResetToDefaultGridLayout();
            _trayContextMenu.Items.Add(resetLayout);

            _trayContextMenu.Items.Add(new Separator());

            // Lock Positions
            var lockItem = new MenuItem
            {
                Header = "Lock Gauge Positions",
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.Current.GlobalLockPositions
            };
            lockItem.Click += (s, e) => _windowManager.SetGlobalLockPositions(lockItem.IsChecked);
            _trayContextMenu.Items.Add(lockItem);

            // Always On Top
            var aotItem = new MenuItem
            {
                Header = "Always On Top",
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.Current.AlwaysOnTop
            };
            aotItem.Click += (s, e) => _windowManager.SetAlwaysOnTop(aotItem.IsChecked);
            _trayContextMenu.Items.Add(aotItem);

            // Start With Windows
            var startupItem = new MenuItem
            {
                Header = "Start with Windows",
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.CheckStartWithWindows()
            };
            startupItem.Click += (s, e) => _windowManager.SetStartWithWindows(startupItem.IsChecked);
            _trayContextMenu.Items.Add(startupItem);

            _trayContextMenu.Items.Add(new Separator());

            // Refresh Rate submenu
            var refreshMenu = new MenuItem { Header = "Refresh Rate" };
            AddRefreshOption(refreshMenu, "Fast (500 ms)", 500);
            AddRefreshOption(refreshMenu, "Normal (1000 ms)", 1000);
            AddRefreshOption(refreshMenu, "Relaxed (2000 ms)", 2000);
            _trayContextMenu.Items.Add(refreshMenu);

            // Gauge Size submenu
            var sizeMenu = new MenuItem { Header = "Gauge Size" };
            AddSizeOption(sizeMenu, "Small (80%)", 0.80);
            AddSizeOption(sizeMenu, "Standard (100%)", 1.0);
            AddSizeOption(sizeMenu, "Large (120%)", 1.20);
            AddSizeOption(sizeMenu, "Extra Large (140%)", 1.40);
            _trayContextMenu.Items.Add(sizeMenu);

            // Opacity submenu
            var opacityMenu = new MenuItem { Header = "Window Opacity" };
            AddOpacityOption(opacityMenu, "100% Solid", 1.0);
            AddOpacityOption(opacityMenu, "90% High", 0.90);
            AddOpacityOption(opacityMenu, "80% Medium", 0.80);
            AddOpacityOption(opacityMenu, "65% Subtle", 0.65);
            AddOpacityOption(opacityMenu, "50% Ghost", 0.50);
            _trayContextMenu.Items.Add(opacityMenu);

            _trayContextMenu.Items.Add(new Separator());

            // Diagnostics
            var diagItem = new MenuItem { Header = "Hardware Sensor Information..." };
            diagItem.Click += (s, e) => _windowManager.ShowHardwareDiagnostics();
            _trayContextMenu.Items.Add(diagItem);

            // Developer Information
            var devInfoItem = new MenuItem { Header = "ⓘ Developer & License Information..." };
            devInfoItem.Click += (s, e) => _windowManager.ShowDeveloperInfo();
            _trayContextMenu.Items.Add(devInfoItem);

            // Exit
            var exitItem = new MenuItem { Header = "Exit Torque Monitor" };
            exitItem.Click += (s, e) => _windowManager.ShutdownApplication();
            _trayContextMenu.Items.Add(exitItem);

            _trayContextMenu.IsOpen = true;
        }

        private void AddRefreshOption(MenuItem parent, string text, int ms)
        {
            var item = new MenuItem
            {
                Header = text,
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.Current.RefreshIntervalMs == ms
            };
            item.Click += (s, e) => _windowManager.SetRefreshInterval(ms);
            parent.Items.Add(item);
        }

        private void AddSizeOption(MenuItem parent, string text, double scale)
        {
            var item = new MenuItem
            {
                Header = text,
                IsCheckable = true,
                IsChecked = Math.Abs((_windowManager.SettingsService.Current.Gauges.FirstOrDefault()?.Scale ?? 1.0) - scale) < 0.05
            };
            item.Click += (s, e) => _windowManager.SetAllGaugesScale(scale);
            parent.Items.Add(item);
        }

        private void AddOpacityOption(MenuItem parent, string text, double opacity)
        {
            var item = new MenuItem
            {
                Header = text,
                IsCheckable = true,
                IsChecked = Math.Abs(_windowManager.SettingsService.Current.GlobalOpacity - opacity) < 0.05
            };
            item.Click += (s, e) => _windowManager.SetGlobalOpacity(opacity);
            parent.Items.Add(item);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (_hwndSource != null)
            {
                var data = new NOTIFYICONDATA
                {
                    cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                    hWnd = _hwndSource.Handle,
                    uID = 1001
                };
                Shell_NotifyIcon(NIM_DELETE, ref data);
                _hwndSource.Dispose();
            }

            if (_hIcon != IntPtr.Zero)
            {
                DestroyIcon(_hIcon);
            }
        }
    }
}
