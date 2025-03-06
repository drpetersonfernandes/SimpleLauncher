using System.Windows;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public partial class BugReportWindow
{
    private static readonly HttpClient HttpClient = new();
    private static string ApiKey { get; set; }

    public BugReportWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        DataContext = this;

        // Load the API key
        LoadConfiguration();
    }

    private static void LoadConfiguration()
    {
        const string configFile = "appsettings.json";
        if (File.Exists(configFile))
        {
            var config = JObject.Parse(File.ReadAllText(configFile));
            ApiKey = config[nameof(ApiKey)]?.ToString();
        }
        else
        {
            // Notify developer
            const string contextMessage = "File 'appsettings.json' is missing.";
            Exception ex = new();
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
            var version2 = (string)Application.Current.TryFindResource("Version") ?? "Version:";
            var unknown2 = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version2 + (version?.ToString() ?? unknown2);
        }
    }

    private async void SendBugReport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var nameText = NameTextBox.Text;
            var emailText = EmailTextBox.Text;
            var bugReportText = BugReportTextBox.Text;
            var applicationVersion = ApplicationVersion;

            if (CheckIfBugReportIsNullOrEmpty(bugReportText)) return;

            var fullMessage = $"\n\n{applicationVersion}\n" +
                              $"Name: {nameText}\n" +
                              $"Email: {emailText}\n" +
                              $"Bug Report:\n\n{bugReportText}";
            await SendBugReportToApiAsync(fullMessage);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the SendBugReport_Click method.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            
        }
    }

    private async Task SendBugReportToApiAsync(string fullMessage)
    {
        // Prepare the POST data
        var formData = new MultipartFormDataContent
        {
            { new StringContent("contact@purelogiccode.com"), "recipient" },
            { new StringContent("Bug Report from SimpleLauncher"), "subject" },
            { new StringContent("SimpleLauncher User"), "name" },
            { new StringContent(fullMessage), "message" }
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
            const string contextMessage = "API Key is not properly loaded from 'appsettings.json'.";
            Exception ex = new();
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ApiKeyErrorMessageBox();

            return;
        }

        try
        {
            var response = await HttpClient.PostAsync("https://purelogiccode.com/simplelauncher/send_email.php", formData);

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
                const string contextMessage = "An error occurred while sending the bug report.";
                Exception ex = new ();
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.BugReportSendErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error sending the bug report.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.BugReportSendErrorMessageBox();
        }
    }

    private static bool CheckIfBugReportIsNullOrEmpty(string bugReportText)
    {
        if (string.IsNullOrWhiteSpace(bugReportText))
        {
            // Notify user
            MessageBoxLibrary.EnterBugDetailsMessageBox();

            return true;
        }

        return false;
    }
}