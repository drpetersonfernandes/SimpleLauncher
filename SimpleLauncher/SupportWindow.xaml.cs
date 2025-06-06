﻿using System;
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

    public SupportWindow()
    {
        InitializeComponent();

        _httpClientFactory = App.ServiceProvider.GetService<IHttpClientFactory>();

        App.ApplyThemeToWindow(this);
        DataContext = this;

        LoadConfiguration();
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
                _ = LogErrors.LogErrorAsync(null, contextMessage);

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
            _ = LogErrors.LogErrorAsync(ex, "There was an error loading 'appsettings.json'.");

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
            _ = LogErrors.LogErrorAsync(null, contextMessage);

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
            var httpClient = _httpClientFactory.CreateClient("SupportWindowClient");
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
                _ = LogErrors.LogErrorAsync(null, contextMessage);

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