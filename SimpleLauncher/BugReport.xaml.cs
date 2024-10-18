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

            App.ApplyThemeToWindow(this);
            
            // Set the data context for data binding
            DataContext = this; 
            
            // Load the API key
            LoadConfiguration(); 
        }
        
        private void LoadConfiguration()
        {
            string configFile = "appsettings.json";
            if (File.Exists(configFile))
            {
                var config = JObject.Parse(File.ReadAllText(configFile));
                ApiKey = config[nameof(ApiKey)]?.ToString();
            }
            else
            {
                string formattedException = $"File appsettings.json is missing in the Bug Report Window.";
                Exception exception = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show("File appsettings.json is missing.\n\nThe application will not be able to send the Bug Report.\n\nPlease reinstall Simple Launcher.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            HttpClient.DefaultRequestHeaders.Remove("X-API-KEY");
            if (!string.IsNullOrEmpty(ApiKey))
            {
                HttpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
            }
            else
            {
                string formattedException = $"API Key is not properly loaded from appsettings.json in the Bug Report Window.";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);
                
                MessageBox.Show("There was an error in the API Key of this form.\n\nThe developer was informed and will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    
                    MessageBox.Show("An error occurred while sending the bug report.\n\nThe bug was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                await LogErrors.LogErrorAsync(ex, $"Error sending the bug report from Bug Report Window.\n\nException detail: {ex.Message}");
                
                MessageBox.Show($"An error occurred while sending the bug report.\n\nThe bug was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}