using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using TorqueMonitor.Models;

namespace TorqueMonitor.Views
{
    public partial class DeveloperInfoDialog : Window
    {
        private readonly DeveloperInfo _info;

        public DeveloperInfoDialog(DeveloperInfo? info = null)
        {
            InitializeComponent();
            _info = info ?? DeveloperInfo.Current;
            PopulateData();
        }

        private void PopulateData()
        {
            // Developer Name
            TxtDeveloperName.Text = _info.DeveloperName;

            // Professional Title
            if (!string.IsNullOrWhiteSpace(_info.ProfessionalTitle))
            {
                TxtProfessionalTitle.Text = _info.ProfessionalTitle;
            }

            // Designation
            if (!string.IsNullOrWhiteSpace(_info.Designation))
            {
                TxtDesignation.Text = _info.Designation;
            }

            // Country
            if (string.IsNullOrWhiteSpace(_info.Country))
            {
                PanelCountry.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtCountry.Text = _info.Country;
                PanelCountry.Visibility = Visibility.Visible;
            }

            // Company
            if (string.IsNullOrWhiteSpace(_info.Company))
            {
                PanelCompany.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtCompany.Text = _info.Company;
                PanelCompany.Visibility = Visibility.Visible;
            }

            // Website
            if (string.IsNullOrWhiteSpace(_info.Website))
            {
                PanelWebsite.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtWebsite.Text = _info.Website;
                PanelWebsite.Visibility = Visibility.Visible;
            }

            // Email
            if (string.IsNullOrWhiteSpace(_info.Email))
            {
                PanelEmail.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtEmail.Text = _info.Email;
                PanelEmail.Visibility = Visibility.Visible;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnWindowDeactivated(object? sender, EventArgs e)
        {
            // Close when clicking outside modal dialog
            try
            {
                Close();
            }
            catch { }
        }

        private void OnWebsiteClick(object sender, MouseButtonEventArgs e)
        {
            OpenSafeUrl(_info.Website);
        }

        private void OnEmailClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_info.Email))
            {
                string mailto = _info.Email.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                    ? _info.Email
                    : $"mailto:{_info.Email}?subject=Inquiry regarding TORQUE MONITOR";
                OpenSafeUrl(mailto);
            }
        }

        private void OnGitHubClick(object sender, RoutedEventArgs e)
        {
            OpenSafeUrl(_info.GitHubUrl);
        }

        private void OnLinkedInClick(object sender, RoutedEventArgs e)
        {
            OpenSafeUrl(_info.LinkedInUrl);
        }

        private void OnFacebookClick(object sender, RoutedEventArgs e)
        {
            OpenSafeUrl(_info.FacebookUrl);
        }

        private static void OpenSafeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeMailto)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = uri.AbsoluteUri,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to open URL '{url}': {ex.Message}");
                    }
                }
            }
        }
    }
}
