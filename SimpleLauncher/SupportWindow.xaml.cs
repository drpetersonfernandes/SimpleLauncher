using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public partial class SupportWindow
{
    private static readonly HttpClient HttpClient = new();
    private static string ApiKey { get; set; }

    public SupportWindow()
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
            var ex = new Exception(contextMessage);
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

    private async void SendSupportRequest_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var nameText = NameTextBox.Text;
            var emailText = EmailTextBox.Text;
            var supportRequestText = SupportTextBox.Text;
            var applicationVersion = ApplicationVersion;

            if (CheckIfNameIsNullOrEmpty(nameText)) return;
            if (CheckIfEmailIsNullOrEmpty(emailText)) return;
            if (CheckIfSupportRequestIsNullOrEmpty(supportRequestText)) return;

            var fullMessage = $"\n\n{applicationVersion}\n" +
                              $"Name: {nameText}\n" +
                              $"Email: {emailText}\n" +
                              $"Bug Report:\n\n{supportRequestText}";
            await SendSupportRequestToApiAsync(fullMessage);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the SendSupportRequest_Click method.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private async Task SendSupportRequestToApiAsync(string fullMessage)
    {
        // Prepare the POST data
        var formData = new MultipartFormDataContent
        {
            { new StringContent("contact@purelogiccode.com"), "recipient" },
            { new StringContent("Support Request from SimpleLauncher"), "subject" },
            { new StringContent("Name"), "name" },
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
            var ex = new Exception(contextMessage);
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
                SupportTextBox.Clear();

                // Notify user
                MessageBoxLibrary.SupportRequestSuccessMessageBox();
            }
            else
            {
                // Notify developer
                const string contextMessage = "An error occurred while sending the Support Request.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SupportRequestSendErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error sending the Support Request.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SupportRequestSendErrorMessageBox();
        }
    }

    private static bool CheckIfNameIsNullOrEmpty(string nameText)
    {
        if (string.IsNullOrWhiteSpace(nameText))
        {
            // Notify user
            MessageBoxLibrary.EnterNameMessageBox();

            return true;
        }

        return false;
    }
    
    private static bool CheckIfEmailIsNullOrEmpty(string emailText)
    {
        if (string.IsNullOrWhiteSpace(emailText))
        {
            // Notify user
            MessageBoxLibrary.EnterEmailMessageBox();

            return true;
        }

        return false;
    }
    
    private static bool CheckIfSupportRequestIsNullOrEmpty(string supportRequestText)
    {
        if (string.IsNullOrWhiteSpace(supportRequestText))
        {
            // Notify user
            MessageBoxLibrary.EnterSupportRequestMessageBox();

            return true;
        }

        return false;
    }
}