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
    private readonly IDebugLogger _debugLogger;

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _supportRequest;
    [ObservableProperty] private bool _isLoading;

    public SupportViewModel(PlaySoundEffects playSoundEffects, IHttpClientFactory httpClientFactory, ILogErrors logErrors, IConfiguration configuration, IMessageBoxLibraryService messageBox, IResourceProvider resourceProvider, IDebugLogger debugLogger)
    {
        _playSoundEffects = playSoundEffects;
        _httpClientFactory = httpClientFactory;
        _logErrors = logErrors;
        _configuration = configuration;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _debugLogger = debugLogger;
    }

    /// <summary>Event raised when the window should be closed.</summary>
    public event Action CloseRequested;

    /// <summary>Event raised when the form fields have been cleared after successful submission.</summary>
    public event Action FormCleared;

    [RelayCommand]
    private async Task SendSupportRequestAsync()
    {
        _debugLogger.Log("[Support] SendSupportRequestAsync started.");

        if (string.IsNullOrWhiteSpace(Name))
        {
            _debugLogger.Log("[Support] Validation failed: Name is empty.");
            await _messageBox.EnterNameMessageBoxAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            _debugLogger.Log("[Support] Validation failed: Email is empty.");
            await _messageBox.EnterEmailMessageBoxAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(SupportRequest))
        {
            _debugLogger.Log("[Support] Validation failed: SupportRequest is empty.");
            await _messageBox.EnterSupportRequestMessageBoxAsync();
            return;
        }

        _debugLogger.Log($"[Support] Validation passed. Name='{Name}', Email='{Email}', MessageLength={SupportRequest.Length}");
        IsLoading = true;

        try
        {
            var fullMessageBuilder = new StringBuilder();
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {Name}");
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Email: {Email}");
            fullMessageBuilder.AppendLine(CultureInfo.InvariantCulture, $"Support Request:\n\n{SupportRequest}");

            _playSoundEffects.PlayNotificationSound();
            _debugLogger.Log("[Support] Calling SendSupportRequestToApiAsync...");
            await SendSupportRequestToApiAsync(fullMessageBuilder.ToString());

            (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
                _resourceProvider.GetString("SendingSupportRequest", "Sending support request..."));
        }
        catch (Exception ex)
        {
            _debugLogger.LogException(ex, "[Support] Exception in SendSupportRequestAsync.");
            _logErrors.LogAndForget(ex, "Error in the SendSupportRequestClickAsync method.");
        }
        finally
        {
            IsLoading = false;
            _debugLogger.Log("[Support] SendSupportRequestAsync finished. IsLoading set to false.");
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
        var supportEmailTo = _configuration.GetValue<string>("SupportEmailTo") ?? "contact@purelogiccode.com";

        _debugLogger.Log($"[Support] EmailApiBaseUrl from config: '{apiBaseUrl}'");
        _debugLogger.Log($"[Support] ApiKey from config: '{apiKey.Substring(0, Math.Min(10, apiKey.Length))}...' (length={apiKey.Length})");
        _debugLogger.Log($"[Support] SupportEmailTo from config: '{supportEmailTo}'");
        _debugLogger.Log($"[Support] Message body length: {fullMessage.Length} chars");

        var requestPayload = new
        {
            to = supportEmailTo,
            subject = "Support Request from SimpleLauncher",
            body = fullMessage,
            applicationName = "SimpleLauncher",
            isHtml = false
        };

        var jsonString = JsonSerializer.Serialize(requestPayload);
        _debugLogger.Log($"[Support] JSON payload size: {jsonString.Length} chars");
        var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        try
        {
            var httpClient = _httpClientFactory?.CreateClient("SupportWindowClient");
            if (httpClient == null)
            {
                _debugLogger.Log("[Support] ERROR: httpClient is null. IHttpClientFactory returned null for 'SupportWindowClient'.");
                return;
            }

            _debugLogger.Log($"[Support] HttpClient created. BaseAddress: '{httpClient.BaseAddress}', Timeout: {httpClient.Timeout}");

            var apiUrl = apiBaseUrl.TrimEnd('/');
            _debugLogger.Log($"[Support] Final API URL: '{apiUrl}'");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Content = jsonContent;
            request.Headers.Add("X-API-KEY", apiKey);

            _debugLogger.Log("[Support] Sending HTTP POST request...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            using var response = await httpClient.SendAsync(request, cts.Token);

            stopwatch.Stop();
            _debugLogger.Log($"[Support] Response received in {stopwatch.ElapsedMilliseconds}ms.");
            _debugLogger.Log($"[Support] StatusCode: {(int)response.StatusCode} ({response.StatusCode})");
            _debugLogger.Log("[Support] Response Headers:");
            foreach (var header in response.Headers)
            {
                _debugLogger.Log($"[Support]   {header.Key}: {string.Join(", ", header.Value)}");
            }

            if (response.Content != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    _debugLogger.Log($"[Support]   {header.Key}: {string.Join(", ", header.Value)}");
                }
            }

            if (response.Content != null)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                _debugLogger.Log($"[Support] Response body ({responseContent.Length} chars): '{responseContent}'");

                if (response.IsSuccessStatusCode)
                {
                    _debugLogger.Log("[Support] SUCCESS: Email sent successfully.");

                    Name = "";
                    Email = "";
                    SupportRequest = "";

                    FormCleared?.Invoke();

                    await _messageBox.SupportRequestSuccessMessageBoxAsync();
                }
                else
                {
                    _debugLogger.Log($"[Support] FAILURE: API returned error. Status={response.StatusCode}, Body='{responseContent}'");

                    var contextMessage = $"An error occurred while sending the Support Request. Status: {response.StatusCode}, Details: {responseContent}";
                    _logErrors.LogAndForget(null, contextMessage);

                    await _messageBox.SupportRequestSendErrorMessageBoxAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _debugLogger.Log("[Support] TIMEOUT: Request timed out after 20 seconds.");
            _logErrors.LogAndForget(null, "The support request timed out after 20 seconds. Please check your internet connection and try again.");

            await _messageBox.SupportRequestSendErrorMessageBoxAsync();
        }
        catch (Exception ex)
        {
            _debugLogger.LogException(ex, "[Support] EXCEPTION: Error sending the Support Request.");
            _logErrors.LogAndForget(ex, "Error sending the Support Request.");

            await _messageBox.SupportRequestSendErrorMessageBoxAsync();
        }
    }
}
