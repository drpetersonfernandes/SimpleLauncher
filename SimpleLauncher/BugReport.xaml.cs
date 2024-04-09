using System.Windows;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher
{
    public partial class BugReport
    {
        private static readonly HttpClient HttpClient = new();
        private static string ApiKey { get; set; }
        
        public BugReport()
        {
            InitializeComponent();
            DataContext = this; // Set the data context for data binding
            LoadConfiguration(); // Load the API key
        }
        
        private void LoadConfiguration()
        {
            string configFile = "appsettings.json";
            if (File.Exists(configFile))
            {
                var config = JObject.Parse(File.ReadAllText(configFile));
                ApiKey = config["ApiKey"]?.ToString();
            }
            else
            {
                MessageBox.Show("Configuration file missing. The application may not function correctly.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static string ApplicationVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return "Version: " + (version?.ToString() ?? "Unknown");
            }
        }

        private async void SendBugReport_Click(object sender, RoutedEventArgs e)
        {
            string nameText = NameTextBox.Text;
            string emailText = EmailTextBox.Text;
            string bugReportText = BugReportTextBox.Text;
            string applicationVersion = ApplicationVersion;

            if (string.IsNullOrWhiteSpace(bugReportText))
            {
                MessageBox.Show("Please enter the details of the bug.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string fullMessage = $"\n\n{applicationVersion}\nName: {nameText}\nEmail: {emailText}\nBug Report:\n\n{bugReportText}";
            await SendBugReportToApiAsync(fullMessage);
        }

        private async Task SendBugReportToApiAsync(string fullMessage)
        {
            string messageWithVersion = fullMessage;

            // Prepare the POST data
            var formData = new MultipartFormDataContent
    {
        { new StringContent("contact@purelogiccode.com"), "recipient" },
        { new StringContent("Bug Report from SimpleLauncher"), "subject" },
        { new StringContent("SimpleLauncher User"), "name" },
        { new StringContent(messageWithVersion), "message" }
    };

            // Set the API Key from the loaded configuration
            HttpClient.DefaultRequestHeaders.Remove("X-API-KEY"); // Remove existing to avoid duplicates
            if (!string.IsNullOrEmpty(ApiKey))
            {
                HttpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
            }
            else
            {
                MessageBox.Show("API Key is not configured. Please check your configuration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                HttpResponseMessage response = await HttpClient.PostAsync("https://purelogiccode.com/simplelauncher/send_email.php", formData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Bug report sent successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    NameTextBox.Clear();
                    EmailTextBox.Clear();
                    BugReportTextBox.Clear();
                }
                else
                {
                    string errorMessage = "An error occurred while sending the bug report.";
                    Exception exception = new Exception(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                await LogErrors.LogErrorAsync(ex, $"Error sending bug report from Bug Report Window.\n\nException detail: {ex}");
                MessageBox.Show($"An error occurred while sending the bug report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}