using System.Diagnostics;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the support option dialog that offers AI-assisted help or developer contact.
/// </summary>
public partial class SupportOptionViewModel : ObservableObject
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;

    private Exception _exception;
    private string _contextMessage;

    public SupportOptionViewModel(PlaySoundEffects playSoundEffects, IConfiguration configuration, ILogErrors logErrors)
    {
        _playSoundEffects = playSoundEffects;
        _configuration = configuration;
        _logErrors = logErrors;
    }

    /// <summary>
    /// Initializes the ViewModel with the exception context for AI-assisted support.
    /// </summary>
    public void Initialize(Exception exception, string contextMessage)
    {
        _exception = exception;
        _contextMessage = contextMessage;
    }

    /// <summary>Event raised when the window should be closed.</summary>
    public event Action CloseRequested;

    [RelayCommand]
    private void ContactDeveloper()
    {
        _playSoundEffects?.PlayNotificationSound();

        var supportRequestWindow = App.ServiceProvider.GetRequiredService<SupportWindow>();
        supportRequestWindow.Show();

        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void AskPerplexity()
    {
        LaunchAiSearch(_configuration.GetValue<string>("Urls:PerplexitySearch") ?? "https://www.perplexity.ai/search?q=");
    }

    [RelayCommand]
    private void AskPhind()
    {
        LaunchAiSearch(_configuration.GetValue<string>("Urls:PhindSearch") ?? "https://www.phind.com/search?q=");
    }

    [RelayCommand]
    private void AskYou()
    {
        LaunchAiSearch(_configuration.GetValue<string>("Urls:YouSearch") ?? "https://you.com/search?q=");
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }

    private void LaunchAiSearch(string baseUrl)
    {
        try
        {
            var query = BuildQuery();
            var encodedQuery = System.Net.WebUtility.UrlEncode(query);
            var url = $"{baseUrl}{encodedQuery}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBoxLibrary.CouldNotOpenBrowserForAiSupportMessageBox();

            var contextMessage = $"Error in LaunchAiSearch with base URL: {baseUrl}";
            _logErrors.LogAndForget(ex, contextMessage);
        }

        CloseRequested?.Invoke();
    }

    private string BuildQuery()
    {
        var sb = new StringBuilder();
        sb.Append("I am a user of the emulator frontend 'Simple Launcher'. It launch emulators via CLI params.");
        sb.Append("I am having trouble launching my game. Please help me out.");
        sb.Append("I do not know if I choose the right core.");
        sb.Append("Maybe the paths are incorrect.");
        sb.Append("Provide me a very simple explanation of the problem and help me fix the parameters.");

        var wikiParametersUrl = _configuration.GetValue<string>("WikiParametersUrl") ?? "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters/";
        if (!string.IsNullOrEmpty(wikiParametersUrl))
        {
            sb.Append(CultureInfo.InvariantCulture, $"'Simple Launcher' parameters reference can be found on {wikiParametersUrl}.");
        }
        else
        {
            sb.Append(" 'Simple Launcher' parameters reference can be found on 'https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters'.");
            _logErrors.LogAndForget(null, "WikiParametersUrl is null or empty in SupportOptionWindow.");
        }

        if (!string.IsNullOrWhiteSpace(_contextMessage))
        {
            sb.Append(CultureInfo.InvariantCulture, $"Context: {_contextMessage}. ");
        }

        if (_exception != null)
        {
            sb.Append(CultureInfo.InvariantCulture, $"Exception Type: {_exception.GetType().Name}. ");
            sb.Append(CultureInfo.InvariantCulture, $"Message: {_exception.Message}. ");

            if (_exception.StackTrace != null)
            {
                var stackTrace = _exception.StackTrace.Length > 1500
                    ? string.Concat(_exception.StackTrace.AsSpan(0, 1500), "...")
                    : _exception.StackTrace;
                sb.Append(CultureInfo.InvariantCulture, $"Stack Trace: {stackTrace}");
            }
        }

        return sb.ToString();
    }
}
