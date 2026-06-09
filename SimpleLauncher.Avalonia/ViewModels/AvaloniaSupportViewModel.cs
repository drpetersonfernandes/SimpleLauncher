using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.PlaySound;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaSupportViewModel : ObservableObject
{
    private readonly IPlaySoundEffects _playSoundEffects;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogErrors _logErrors;
    private readonly IConfiguration _configuration;
    private readonly IMessageBoxLibraryService _messageBox;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _supportRequest = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public AvaloniaSupportViewModel(
        IPlaySoundEffects playSoundEffects,
        IHttpClientFactory httpClientFactory,
        ILogErrors logErrors,
        IConfiguration configuration,
        IMessageBoxLibraryService messageBox)
    {
        _playSoundEffects = playSoundEffects;
        _httpClientFactory = httpClientFactory;
        _logErrors = logErrors;
        _configuration = configuration;
        _messageBox = messageBox;
    }

    public event Action? CloseRequested;
    public event Action? FormCleared;

    [RelayCommand]
    private async Task SendSupportRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await _messageBox.EnterNameMessageBox();
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            await _messageBox.EnterEmailMessageBox();
            return;
        }

        if (string.IsNullOrWhiteSpace(SupportRequest))
        {
            await _messageBox.EnterSupportRequestMessageBox();
            return;
        }

        IsLoading = true;

        try
        {
            var fullMessageBuilder = new StringBuilder();
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {Name}");
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Email: {Email}");
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Support Request:\n\n{SupportRequest}");

            _playSoundEffects.PlayNotificationSound();
            await SendSupportRequestToApiAsync(fullMessageBuilder.ToString());
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error in the SendSupportRequestAsync method.";
            _logErrors.LogAndForget(ex, contextMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke();
    }

    private async Task SendSupportRequestToApiAsync(string fullMessage)
    {
        var apiBaseUrl = _configuration.GetValue<string>("EmailApiBaseUrl") ?? "https://www.purelogiccode.com/customeremailservice/api/send-customer-email/";
        var apiKey = _configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";

        if (string.IsNullOrEmpty(apiBaseUrl))
        {
            await _messageBox.ApiKeyErrorMessageBox();
            return;
        }

        var requestPayload = new
        {
            to = "contact@purelogiccode.com",
            subject = "Support Request from SimpleLauncher",
            body = fullMessage,
            applicationName = "SimpleLauncher",
            isHtml = false
        };

        var jsonString = JsonSerializer.Serialize(requestPayload);
        var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        try
        {
            var httpClient = _httpClientFactory?.CreateClient("SupportWindowClient");
            if (httpClient != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                var apiUrl = apiBaseUrl.TrimEnd('/');
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                using var response = await httpClient.PostAsync(apiUrl, jsonContent, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    Name = string.Empty;
                    Email = string.Empty;
                    SupportRequest = string.Empty;
                    FormCleared?.Invoke();
                    await _messageBox.SupportRequestSuccessMessageBox();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                    var contextMessage = $"An error occurred while sending the Support Request. Status: {response.StatusCode}, Details: {errorContent}";
                    _logErrors.LogAndForget(null, contextMessage);
                    await _messageBox.SupportRequestSendErrorMessageBox();
                }
            }
        }
        catch (OperationCanceledException)
        {
            const string contextMessage = "The support request timed out after 20 seconds. Please check your internet connection and try again.";
            _logErrors.LogAndForget(null, contextMessage);
            await _messageBox.SupportRequestSendErrorMessageBox();
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error sending the Support Request.";
            _logErrors.LogAndForget(ex, contextMessage);
            await _messageBox.SupportRequestSendErrorMessageBox();
        }
    }
}
