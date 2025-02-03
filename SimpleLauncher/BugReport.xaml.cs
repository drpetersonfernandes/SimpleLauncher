using System.Windows;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public partial class BugReport
{
    private static readonly HttpClient HttpClient = new();
    private static string ApiKey { get; set; }
        
    public BugReport()
    {
        InitializeComponent();

        App.ApplyThemeToWindow(this);
            
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
            // Notify developer
            string formattedException = $"File 'appsettings.json' is missing.";
            Exception exception = new(formattedException);
            Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
    
            // Notify user
            MessageBoxLibrary.RequiredFileMissingMessageBox();
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
            string version2 = (string)Application.Current.TryFindResource("Version") ?? "Version:";
            string unknown2 = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version2 + (version?.ToString() ?? unknown2);
        }
    }

    private async void SendBugReport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string nameText = NameTextBox.Text;
            string emailText = EmailTextBox.Text;
            string bugReportText = BugReportTextBox.Text;
            string applicationVersion = ApplicationVersion;

            if (string.IsNullOrWhiteSpace(bugReportText))
            {
                // Notify user
                MessageBoxLibrary.EnterBugDetailsMessageBox();

                return;
            }

            string fullMessage = $"\n\n{applicationVersion}\n" +
                                 $"Name: {nameText}\n" +
                                 $"Email: {emailText}\n" +
                                 $"Bug Report:\n\n{bugReportText}";
            await SendBugReportToApiAsync(fullMessage);
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error in the SendBugReport_Click method.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);            }
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
            // Notify developer
            string formattedException = "API Key is not properly loaded from 'appsettings.json'.";
            Exception exception = new(formattedException);
            await LogErrors.LogErrorAsync(exception, formattedException);

            // Notify user
            MessageBoxLibrary.ApiKeyErrorMessageBox();

            return;
        }

        try
        {
            HttpResponseMessage response = await HttpClient.PostAsync("https://purelogiccode.com/simplelauncher/send_email.php", formData);

            if (response.IsSuccessStatusCode)
            {
                NameTextBox.Clear();
                EmailTextBox.Clear();
                BugReportTextBox.Clear();
                
                // Notify user
                MessageBoxLibrary.BugReportSuccessMessageBox();
            }
            else
            {
                // Notify developer
                string errorMessage = "An error occurred while sending the bug report.";
                Exception exception = new Exception(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                
                // Notify user
                MessageBoxLibrary.BugReportSendErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            await LogErrors.LogErrorAsync(ex, $"Error sending the bug report.\n\n" +
                                              $"Exception type: {ex.GetType().Name}\n" +
                                              $"Exception details: {ex.Message}");

            // Notify user
            MessageBoxLibrary.BugReportSendErrorMessageBox();
        }
    }
}