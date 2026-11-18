using System.Windows;

namespace TorqueMonitor.Views
{
    public partial class HardwareInfoDialog : Window
    {
        public HardwareInfoDialog(string diagnosticsText)
        {
            InitializeComponent();
            TxtDiagnostics.Text = diagnosticsText;
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtDiagnostics.Text);
                MessageBox.Show("Diagnostics copied to clipboard.", "Torque Monitor", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnDeveloperInfoClick(object sender, RoutedEventArgs e)
        {
            var dlg = new DeveloperInfoDialog();
            dlg.Show();
        }
    }
}
