using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TorqueMonitor.Models;
using TorqueMonitor.ViewModels;

namespace TorqueMonitor.Views
{
    public partial class GaugeWindow : Window
    {
        private readonly WindowManager _windowManager;
        private readonly GaugeSettings _settings;

        public GaugeViewModel ViewModel { get; }
        public GaugeType GaugeType => _settings.Type;

        public GaugeWindow(GaugeViewModel viewModel, GaugeSettings settings, WindowManager windowManager)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _settings = settings;
            _windowManager = windowManager;

            DataContext = ViewModel;
            GaugeDisplay.DataContext = ViewModel;

            ApplySettings();

            // Set rendering optimizations with full anti-aliased smooth edges
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            Left = _settings.X;
            Top = _settings.Y;
            ApplyScale(_settings.Scale);
            Opacity = _windowManager.SettingsService.Current.GlobalOpacity;
            Topmost = _windowManager.SettingsService.Current.AlwaysOnTop;
            Visibility = _settings.IsVisible ? Visibility.Visible : Visibility.Collapsed;

            // Update cursor based on lock state
            GaugeHostBorder.Cursor = (_settings.IsLocked || _windowManager.SettingsService.Current.GlobalLockPositions)
                ? Cursors.Arrow
                : Cursors.SizeAll;
        }

        public void ApplyScale(double scale)
        {
            _settings.Scale = Math.Clamp(scale, 0.6, 2.0);
            double newDimension = 240.0 * _settings.Scale;
            Width = newDimension;
            Height = newDimension;
            WindowScaleTransform.ScaleX = _settings.Scale;
            WindowScaleTransform.ScaleY = _settings.Scale;
        }

        private void OnGaugeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_settings.IsLocked || _windowManager.SettingsService.Current.GlobalLockPositions)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch { }

                // DragMove has finished and user released mouse:
                _settings.X = Left;
                _settings.Y = Top;
                _windowManager.SettingsService.ClampGaugePosition(_settings, 240.0 * _settings.Scale);
                Left = _settings.X;
                Top = _settings.Y;
                _windowManager.SaveSettings();
            }
        }

        private void OnGaugeMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Mouse wheel directly on the gauge scales this meter smoothly
            double delta = e.Delta > 0 ? 0.05 : -0.05;
            ApplyScale(_settings.Scale + delta);
            _windowManager.SaveSettings();
            e.Handled = true;
        }

        private void OnGaugeMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowContextMenu();
        }

        public void ShowContextMenu()
        {
            var menu = new ContextMenu();

            // Header: Gauge Title
            var titleItem = new MenuItem
            {
                Header = $"TORQUE MONITOR — {ViewModel.Title}",
                IsEnabled = false,
                FontWeight = FontWeights.Bold
            };
            menu.Items.Add(titleItem);
            menu.Items.Add(new Separator());

            // 1. Hide this Gauge
            var hideItem = new MenuItem { Header = $"Hide {ViewModel.Title} Gauge" };
            hideItem.Click += (s, e) =>
            {
                _settings.IsVisible = false;
                Visibility = Visibility.Collapsed;
                _windowManager.SaveSettings();
            };
            menu.Items.Add(hideItem);

            // 2. Show All 6 Primary Gauges
            var showAllItem = new MenuItem { Header = "Show All 6 Primary Gauges" };
            showAllItem.Click += (s, e) => _windowManager.ShowAllPrimaryGauges();
            menu.Items.Add(showAllItem);

            // 3. Gauge Visibility Submenu
            var visibilityMenu = new MenuItem { Header = "Gauge Visibility" };
            foreach (var gaugeWindow in _windowManager.AllGaugeWindows)
            {
                var gItem = new MenuItem
                {
                    Header = gaugeWindow.ViewModel.Title,
                    IsCheckable = true,
                    IsChecked = gaugeWindow.Visibility == Visibility.Visible
                };
                var targetG = gaugeWindow;
                gItem.Click += (s, e) =>
                {
                    targetG._settings.IsVisible = gItem.IsChecked;
                    targetG.Visibility = gItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                    _windowManager.SaveSettings();
                };
                visibilityMenu.Items.Add(gItem);
            }
            menu.Items.Add(visibilityMenu);

            menu.Items.Add(new Separator());

            // 4. Gauge Size Presets for THIS Gauge
            var sizeThisMenu = new MenuItem { Header = "Gauge Size (This Gauge)" };
            AddThisGaugeSizeOption(sizeThisMenu, "Small (80%)", 0.80);
            AddThisGaugeSizeOption(sizeThisMenu, "Standard (100%)", 1.0);
            AddThisGaugeSizeOption(sizeThisMenu, "Large (120%)", 1.20);
            AddThisGaugeSizeOption(sizeThisMenu, "Extra Large (140%)", 1.40);
            menu.Items.Add(sizeThisMenu);

            // 5. Gauge Size Presets for ALL Gauges
            var sizeAllMenu = new MenuItem { Header = "Gauge Size (All Gauges)" };
            AddAllGaugesSizeOption(sizeAllMenu, "Small (80%)", 0.80);
            AddAllGaugesSizeOption(sizeAllMenu, "Standard (100%)", 1.0);
            AddAllGaugesSizeOption(sizeAllMenu, "Large (120%)", 1.20);
            AddAllGaugesSizeOption(sizeAllMenu, "Extra Large (140%)", 1.40);
            menu.Items.Add(sizeAllMenu);

            // 6. Global Opacity
            var opacityMenu = new MenuItem { Header = "Window Opacity" };
            AddOpacityOption(opacityMenu, "100% Solid", 1.0);
            AddOpacityOption(opacityMenu, "90% High", 0.90);
            AddOpacityOption(opacityMenu, "80% Medium", 0.80);
            AddOpacityOption(opacityMenu, "65% Subtle", 0.65);
            AddOpacityOption(opacityMenu, "50% Ghost", 0.50);
            menu.Items.Add(opacityMenu);

            // 7. Lock Position
            var lockItem = new MenuItem
            {
                Header = "Lock Gauge Positions",
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.Current.GlobalLockPositions
            };
            lockItem.Click += (s, e) =>
            {
                _windowManager.SetGlobalLockPositions(lockItem.IsChecked);
            };
            menu.Items.Add(lockItem);

            // 8. Reset to 3x2 Grid Layout
            var resetLayoutItem = new MenuItem { Header = "Reset to 3x2 Grid Layout" };
            resetLayoutItem.Click += (s, e) => _windowManager.ResetToDefaultGridLayout();
            menu.Items.Add(resetLayoutItem);

            menu.Items.Add(new Separator());

            // 9. Always On Top
            var aotItem = new MenuItem
            {
                Header = "Always On Top",
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.Current.AlwaysOnTop
            };
            aotItem.Click += (s, e) => _windowManager.SetAlwaysOnTop(aotItem.IsChecked);
            menu.Items.Add(aotItem);

            // 10. Refresh Interval Submenu
            var refreshMenu = new MenuItem { Header = "Refresh Rate" };
            AddRefreshOption(refreshMenu, "Fast (500 ms)", 500);
            AddRefreshOption(refreshMenu, "Normal (1000 ms)", 1000);
            AddRefreshOption(refreshMenu, "Relaxed (2000 ms)", 2000);
            menu.Items.Add(refreshMenu);

            // 11. Start with Windows
            var startupItem = new MenuItem
            {
                Header = "Start with Windows",
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.CheckStartWithWindows()
            };
            startupItem.Click += (s, e) => _windowManager.SetStartWithWindows(startupItem.IsChecked);
            menu.Items.Add(startupItem);

            menu.Items.Add(new Separator());

            // 12. Hardware Diagnostics
            var diagItem = new MenuItem { Header = "Hardware Sensor Information..." };
            diagItem.Click += (s, e) => _windowManager.ShowHardwareDiagnostics();
            menu.Items.Add(diagItem);

            // 13. Developer Information
            var devInfoItem = new MenuItem { Header = "ⓘ Developer & License Information..." };
            devInfoItem.Click += (s, e) => _windowManager.ShowDeveloperInfo();
            menu.Items.Add(devInfoItem);

            // 14. Exit
            var exitItem = new MenuItem { Header = "Exit Torque Monitor" };
            exitItem.Click += (s, e) => _windowManager.ShutdownApplication();
            menu.Items.Add(exitItem);

            menu.PlacementTarget = GaugeHostBorder;
            menu.IsOpen = true;
        }

        private void AddThisGaugeSizeOption(MenuItem parent, string label, double scale)
        {
            var item = new MenuItem
            {
                Header = label,
                IsCheckable = true,
                IsChecked = Math.Abs(_settings.Scale - scale) < 0.05
            };
            item.Click += (s, e) =>
            {
                ApplyScale(scale);
                _windowManager.SaveSettings();
            };
            parent.Items.Add(item);
        }

        private void AddAllGaugesSizeOption(MenuItem parent, string label, double scale)
        {
            var item = new MenuItem
            {
                Header = label,
                IsCheckable = true,
                IsChecked = Math.Abs(_settings.Scale - scale) < 0.05
            };
            item.Click += (s, e) =>
            {
                _windowManager.SetAllGaugesScale(scale);
            };
            parent.Items.Add(item);
        }

        private void AddOpacityOption(MenuItem parent, string label, double opacity)
        {
            var item = new MenuItem
            {
                Header = label,
                IsCheckable = true,
                IsChecked = Math.Abs(_windowManager.SettingsService.Current.GlobalOpacity - opacity) < 0.05
            };
            item.Click += (s, e) =>
            {
                _windowManager.SetGlobalOpacity(opacity);
            };
            parent.Items.Add(item);
        }

        private void AddRefreshOption(MenuItem parent, string label, int ms)
        {
            var item = new MenuItem
            {
                Header = label,
                IsCheckable = true,
                IsChecked = _windowManager.SettingsService.Current.RefreshIntervalMs == ms
            };
            item.Click += (s, e) =>
            {
                _windowManager.SetRefreshInterval(ms);
            };
            parent.Items.Add(item);
        }
    }
}
