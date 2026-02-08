using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher;

public partial class SupportWindow : ILoadingState
{
    private readonly PlaySoundEffects _playSoundEffects;
    private static IHttpClientFactory _httpClientFactory;
    private readonly ILogErrors _logErrors;
    private readonly Exception _exception;
    private readonly string _contextMessage;
    private readonly IConfiguration _configuration;

    public SupportWindow(PlaySoundEffects playSoundEffects, IHttpClientFactory httpClientFactory, ILogErrors logErrors, Exception exception, string contextMessage, IConfiguration configuration)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _playSoundEffects = playSoundEffects;
        _httpClientFactory = httpClientFactory;
        _logErrors = logErrors;
        _exception = exception;
        _contextMessage = contextMessage;
        _configuration = configuration;

        MessageBuilder();
    }

    private void MessageBuilder()
    {
        var messageBuilder = new StringBuilder();
        var applicationVersion = Services.GetApplicationVersion.GetApplicationVersion.GetVersion ?? "Unknown";

        // Add a header to indicate this is an automatically generated report
        messageBuilder.AppendLine("--- Automatically Generated Error Report ---");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Date: {DateTime.Now}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Application Version: {applicationVersion}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"OS: {Environment.OSVersion.VersionString}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Architecture: {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
        messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Bitness: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
        messageBuilder.AppendLine("------------------------------------------\n");

        if (!string.IsNullOrEmpty(_contextMessage))
        {
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Context Message: {_contextMessage}");
            messageBuilder.AppendLine(); // Add a blank line for readability
        }

        if (_exception != null)
        {
            messageBuilder.AppendLine("--- Exception Details ---");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Type: {_exception.GetType().FullName}");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Message: {_exception.Message}");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Source: {_exception.Source}");
            messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace:\n{_exception.StackTrace}");

            if (_exception.InnerException != null)
            {
                messageBuilder.AppendLine("\n--- Inner Exception ---");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Type: {_exception.InnerException.GetType().FullName}");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Message: {_exception.InnerException.Message}");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Source: {_exception.InnerException.Source}");
                messageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace:\n{_exception.InnerException.StackTrace}");
                messageBuilder.AppendLine("-----------------------");
            }

            messageBuilder.AppendLine("-----------------------");
        }

        // Set the text of the SupportTextBox
        SupportTextBox.Text = messageBuilder.ToString();
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            // Ensure the main content area is disabled to prevent Tab-key navigation
            MainContentGrid?.IsEnabled = !isLoading;

            if (isLoading)
            {
                LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void SendSupportRequestClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            var nameText = NameTextBox.Text;
            var emailText = EmailTextBox.Text;
            var supportRequestText = SupportTextBox.Text; // This now includes the pre-filled error report

            if (CheckIfNameIsNullOrEmpty(nameText)) return;
            if (CheckIfEmailIsNullOrEmpty(emailText)) return;
            if (CheckIfSupportRequestIsNullOrEmpty(supportRequestText)) return; // Check if it's still empty after pre-filling

            MainContentGrid.IsEnabled = false;

            LoadingOverlay.Content = (string)Application.Current.TryFindResource("SendingMessagePleaseWait") ?? "Sending message... Please wait.";
            LoadingOverlay.Visibility = Visibility.Visible;

            await Task.Yield(); // Allow UI to render the progress overlay

            try
            {
                // Build the full message, including original error details if available
                // The SupportTextBox.Text already contains the formatted error details if provided
                var fullMessageBuilder = new StringBuilder();
                fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {nameText}");
                fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Email: {emailText}");
                fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Support Request:\n\n{supportRequestText}"); // Use the content of the textbox directly

                _playSoundEffects.PlayNotificationSound();
                await SendSupportRequestToApiAsync(fullMessageBuilder.ToString());

                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SendingSupportRequest") ?? "Sending support request...", Application.Current.MainWindow as MainWindow);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error in the SendSupportRequestClickAsync method.";
                _ = _logErrors.LogErrorAsync(ex, contextMessage);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                MainContentGrid.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the SendSupportRequestClickAsync method.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private async Task SendSupportRequestToApiAsync(string fullMessage)
    {
        var apiBaseUrl = _configuration.GetValue<string>("EmailApiBaseUrl") ?? "https://www.purelogiccode.com/customeremailservice/api/send-customer-email/";
        var apiKey = _configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";

        // Check if the API base URL is configured
        if (string.IsNullOrEmpty(apiBaseUrl))
        {
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
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                // Construct the full API URL
                var apiUrl = apiBaseUrl.TrimEnd('/');

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
                    _ = _logErrors.LogErrorAsync(null, contextMessage);

                    // Notify user
                    MessageBoxLibrary.SupportRequestSendErrorMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error sending the Support Request.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

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

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        LoadingOverlay.Visibility = Visibility.Collapsed;
        MainContentGrid?.IsEnabled = true;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in SupportWindow.");
        UpdateStatusBar.UpdateContent("Emergency reset performed.", Application.Current.MainWindow as MainWindow);
    }
}