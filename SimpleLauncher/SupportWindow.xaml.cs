using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public partial class SupportWindow
{
    private static readonly HttpClient HttpClient = new();
    private static string ApiKey { get; set; }
    private static string ApiBaseUrl { get; set; }

    public SupportWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        DataContext = this;

        // Load the API key and base URL
        LoadConfiguration();
    }

    private static void LoadConfiguration()
    {
        const string configFile = "appsettings.json";
        if (File.Exists(configFile))
        {
            var config = JObject.Parse(File.ReadAllText(configFile));
            ApiKey = config[nameof(ApiKey)]?.ToString();
            ApiBaseUrl = config["EmailApiBaseUrl"]?.ToString() ?? "https://www.purelogiccode.com/customeremailservice"; // Default if not specified
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
                              $"Support Request:\n\n{supportRequestText}";
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
        // Check if the API base URL is configured
        if (string.IsNullOrEmpty(ApiBaseUrl))
        {
            // Notify developer
            const string contextMessage = "Email API base URL is not properly configured in 'appsettings.json'.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ApiKeyErrorMessageBox();
            return;
        }

        // Create the request payload for the new API
        var requestPayload = new
        {
            to = "contact@purelogiccode.com",
            subject = "Support Request from SimpleLauncher",
            body = fullMessage,
            applicationName = "SimpleLauncher",
            isHtml = false
        };

        // Convert to JSON
        var jsonContent = new StringContent(
            JsonConvert.SerializeObject(requestPayload),
            Encoding.UTF8,
            "application/json");

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
            // Construct the full API URL
            var apiUrl = $"{ApiBaseUrl.TrimEnd('/')}/api/send-customer-email";

            using var response = await HttpClient.PostAsync(apiUrl, jsonContent);

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
                // Get error details from response
                var errorContent = await response.Content.ReadAsStringAsync();

                // Notify developer
                var contextMessage = $"An error occurred while sending the Support Request. Status: {response.StatusCode}, Details: {errorContent}";
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