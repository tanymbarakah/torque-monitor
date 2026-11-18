using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;
using TorqueMonitor.Models;

namespace TorqueMonitor.Services
{
    public class SettingsService
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TorqueMonitor");
        private static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.json");
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "TorqueMonitor";

        public AppSettings Current { get; private set; }

        public SettingsService()
        {
            Current = LoadSettings();
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        ValidateSettings(settings);
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            var defaultSettings = AppSettings.CreateDefault();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        public void SaveSettings(AppSettings? settings = null)
        {
            try
            {
                settings ??= Current;
                if (!Directory.Exists(AppDataFolder))
                {
                    Directory.CreateDirectory(AppDataFolder);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private void ValidateSettings(AppSettings settings)
        {
            var defaultSettings = AppSettings.CreateDefault();
            foreach (var defaultGauge in defaultSettings.Gauges)
            {
                if (!settings.Gauges.Any(g => g.Type == defaultGauge.Type))
                {
                    settings.Gauges.Add(defaultGauge);
                }
            }
        }

        public void SetStartWithWindows(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                if (key != null)
                {
                    string? exePath = Environment.ProcessPath;
                    if (enable && !string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppName, $"\"{exePath}\"");
                        Current.StartWithWindows = true;
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                        Current.StartWithWindows = false;
                    }
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update registry startup key: {ex.Message}");
            }
        }

        public bool CheckStartWithWindows()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        public void ClampGaugePosition(GaugeSettings gauge, double gaugePixelSize)
        {
            // Verify against VirtualScreen bounds to handle multi-monitor setups
            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualWidth = SystemParameters.VirtualScreenWidth;
            double virtualHeight = SystemParameters.VirtualScreenHeight;

            double minX = virtualLeft;
            double maxX = virtualLeft + virtualWidth - gaugePixelSize;
            double minY = virtualTop;
            double maxY = virtualTop + virtualHeight - gaugePixelSize;

            if (gauge.X < minX || gauge.X > maxX)
            {
                gauge.X = Math.Clamp(gauge.X, minX, Math.Max(minX, maxX));
            }
            if (gauge.Y < minY || gauge.Y > maxY)
            {
                gauge.Y = Math.Clamp(gauge.Y, minY, Math.Max(minY, maxY));
            }
        }
    }
}
