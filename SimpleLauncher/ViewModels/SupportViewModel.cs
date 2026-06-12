using System.Globalization;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.PlaySound;
using Application = System.Windows.Application;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the support request submission window.
/// </summary>
public partial class SupportViewModel : ObservableObject
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogErrors _logErrors;
    private readonly IConfiguration _configuration;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _supportRequest;
    [ObservableProperty] private bool _isLoading;

    public SupportViewModel(PlaySoundEffects playSoundEffects, IHttpClientFactory httpClientFactory, ILogErrors logErrors, IConfiguration configuration, IMessageBoxLibraryService messageBox, IResourceProvider resourceProvider)
    {
        _playSoundEffects = playSoundEffects;
        _httpClientFactory = httpClientFactory;
        _logErrors = logErrors;
        _configuration = configuration;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
    }

    /// <summary>Event raised when the window should be closed.</summary>
    public event Action CloseRequested;

    /// <summary>Event raised when the form fields have been cleared after successful submission.</summary>
    public event Action FormCleared;

    [RelayCommand]
    private async Task SendSupportRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await _messageBox.EnterNameMessageBoxAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            await _messageBox.EnterEmailMessageBoxAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(SupportRequest))
        {
            await _messageBox.EnterSupportRequestMessageBoxAsync();
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

            (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
                _resourceProvider.GetString("SendingSupportRequest", "Sending support request..."));
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error in the SendSupportRequestClickAsync method.";
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
                var apiUrl = apiBaseUrl.TrimEnd('/');

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

                using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Content = jsonContent;
                request.Headers.Add("X-API-KEY", apiKey);

                using var response = await httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    Name = "";
                    Email = "";
                    SupportRequest = "";

                    FormCleared?.Invoke();

                    await _messageBox.SupportRequestSuccessMessageBoxAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cts.Token);

                    var contextMessage = $"An error occurred while sending the Support Request. Status: {response.StatusCode}, Details: {errorContent}";
                    _logErrors.LogAndForget(null, contextMessage);

                    await _messageBox.SupportRequestSendErrorMessageBoxAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            const string contextMessage = "The support request timed out after 20 seconds. Please check your internet connection and try again.";
            _logErrors.LogAndForget(null, contextMessage);

            await _messageBox.SupportRequestSendErrorMessageBoxAsync();
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error sending the Support Request.";
            _logErrors.LogAndForget(ex, contextMessage);

            await _messageBox.SupportRequestSendErrorMessageBoxAsync();
        }
    }
}
