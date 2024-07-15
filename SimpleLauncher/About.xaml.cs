using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Reflection;

namespace SimpleLauncher
{
    public partial class About
    {
        public About()
        {
            InitializeComponent();
            
            // Apply the theme to this window
            App.ApplyThemeToWindow(this);
            
            // Set the data context for data binding
            DataContext = this;
            
            // Set the AppVersionTextBlock 
            AppVersionTextBlock.Text = ApplicationVersion;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private async void CheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            await UpdateChecker.CheckForUpdatesAsync2(this);
        }

        private static string ApplicationVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return "Version: " + (version?.ToString() ?? "Unknown");
            }
        }
    }
}
