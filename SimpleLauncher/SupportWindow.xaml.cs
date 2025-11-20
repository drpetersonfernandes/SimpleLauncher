using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class SupportWindow
{
    private static IHttpClientFactory _httpClientFactory;
    private static string ApiKey { get; set; }
    private static string ApiBaseUrl { get; set; }
    public Exception OriginalException { get; set; }
    public string OriginalContextMessage { get; set; }
    public GameLauncher GameLauncher { get; }

    public SupportWindow(GameLauncher gameLauncher = null)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        DataContext = this;
        GameLauncher = gameLauncher;
        LoadConfiguration();
    }

    // Constructor overload to receive exception and context message
    public SupportWindow(Exception ex, string contextMessage, GameLauncher gameLauncher) : this(gameLauncher) // Call the default constructor first
    {
        OriginalException = ex; // Store the exception object
        OriginalContextMessage = contextMessage; // Store the context message

        // Populate SupportTextBox with exception details and context message
        var messageBuilder = new StringBuilder();

        // Add a header to indicate this is an automatically generated report
        messageBuilder.AppendLine("--- Automatically Generated Error Report ---");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Date: {DateTime.Now}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Application Version: {ApplicationVersion}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"OS: {Environment.OSVersion.VersionString}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Architecture: {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Bitness: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
        messageBuilder.AppendLine("------------------------------------------\n");


        if (!string.IsNullOrEmpty(contextMessage))
        {
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Context Message: {contextMessage}");
            messageBuilder.AppendLine(); // Add a blank line for readability
        }

        if (ex != null)
        {
            messageBuilder.AppendLine("--- Exception Details ---");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Type: {ex.GetType().FullName}");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Message: {ex.Message}");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Source: {ex.Source}");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace:\n{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                messageBuilder.AppendLine("\n--- Inner Exception ---");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Type: {ex.InnerException.GetType().FullName}");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Message: {ex.InnerException.Message}");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Source: {ex.InnerException.Source}");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace:\n{ex.InnerException.StackTrace}");
                messageBuilder.AppendLine("-----------------------");
            }

            messageBuilder.AppendLine("-----------------------");
        }

        // Set the text of the SupportTextBox
        SupportTextBox.Text = messageBuilder.ToString();
    }

    private static void LoadConfiguration()
    {
        const string configFile = "appsettings.json";
        try
        {
            if (!File.Exists(configFile))
            {
                // Notify developer
                const string contextMessage = "File 'appsettings.json' is missing.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.RequiredFileMissingMessageBox();

                return;
            }

            var configText = File.ReadAllText(configFile);
            using var jsonDoc = JsonDocument.Parse(configText);
            var root = jsonDoc.RootElement;

            // Extract API Key with null check
            if (root.TryGetProperty(nameof(ApiKey), out var apiKeyElement))
            {
                ApiKey = apiKeyElement.GetString();
            }
            else
            {
                throw new InvalidOperationException("ApiKey is missing in configuration");
            }

            // Extract API Base URL with default value
            ApiBaseUrl = root.TryGetProperty("EmailApiBaseUrl", out var urlProp)
                ? urlProp.GetString()
                : "https://www.purelogiccode.com/customeremailservice";
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "There was an error loading 'appsettings.json'.");

            // Notify user
            MessageBoxLibrary.ErrorLoadingAppSettingsMessageBox();
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
            var supportRequestText = SupportTextBox.Text; // This now includes the pre-filled error report

            if (CheckIfNameIsNullOrEmpty(nameText)) return;
            if (CheckIfEmailIsNullOrEmpty(emailText)) return;
            if (CheckIfSupportRequestIsNullOrEmpty(supportRequestText)) return; // Check if it's still empty after pre-filling

            // Build the full message, including original error details if available
            // The SupportTextBox.Text already contains the formatted error details if provided
            var fullMessageBuilder = new StringBuilder();
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {nameText}");
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Email: {emailText}");
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Support Request:\n\n{supportRequestText}"); // Use the content of the textbox directly

            await SendSupportRequestToApiAsync(fullMessageBuilder.ToString());

            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SendingSupportRequest") ?? "Sending support request...", Application.Current.MainWindow as MainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the SendSupportRequest_Click method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }

    private async Task SendSupportRequestToApiAsync(string fullMessage)
    {
        // Check if the API base URL is configured
        if (string.IsNullOrEmpty(ApiBaseUrl))
        {
            // Notify developer
            const string contextMessage = "Email API base URL is not properly configured in 'appsettings.json'.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

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

        // Convert to JSON using System.Text.Json
        var jsonString = JsonSerializer.Serialize(requestPayload);
        var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        try
        {
            var httpClient = _httpClientFactory?.CreateClient("SupportWindowClient");
            if (httpClient != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);

                // Construct the full API URL
                var apiUrl = $"{ApiBaseUrl.TrimEnd('/')}";

                using var response = await httpClient.PostAsync(apiUrl, jsonContent);

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
                    // Get error details from the response
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Notify developer
                    var contextMessage = $"An error occurred while sending the Support Request. Status: {response.StatusCode}, Details: {errorContent}";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                    // Notify user
                    MessageBoxLibrary.SupportRequestSendErrorMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error sending the Support Request.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SupportRequestSendErrorMessageBox();
        }
    }

    private static bool CheckIfNameIsNullOrEmpty(string nameText)
    {
        if (!string.IsNullOrWhiteSpace(nameText)) return false;

        // Notify user
        MessageBoxLibrary.EnterNameMessageBox();

        return true;
    }

    private static bool CheckIfEmailIsNullOrEmpty(string emailText)
    {
        if (!string.IsNullOrWhiteSpace(emailText)) return false;

        // Notify user
        MessageBoxLibrary.EnterEmailMessageBox();

        return true;
    }

    private static bool CheckIfSupportRequestIsNullOrEmpty(string supportRequestText)
    {
        if (!string.IsNullOrWhiteSpace(supportRequestText)) return false;

        // Notify user
        MessageBoxLibrary.EnterSupportRequestMessageBox();

        return true;
    }
}