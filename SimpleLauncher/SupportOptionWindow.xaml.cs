using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher;

public partial class SupportOptionWindow
{
    private readonly Exception _exception;
    private readonly string _contextMessage;
    private readonly GameLauncher _gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IConfiguration _configuration;

    public SupportOptionWindow(Exception ex, string contextMessage, GameLauncher gameLauncher, PlaySoundEffects playSoundEffects, IConfiguration configuration)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _exception = ex;
        _contextMessage = contextMessage;
        _gameLauncher = gameLauncher;
        _playSoundEffects = playSoundEffects;
        _configuration = configuration;

        // Set fallback text if resources are missing
        if (Title is null or "SupportOptions")
        {
            Title = (string)Application.Current.TryFindResource("SupportOptions") ?? "Support Options";
        }

        if (BtnContactDeveloper.Content == null)
        {
            BtnContactDeveloper.Content = (string)Application.Current.TryFindResource("ContactDeveloperReportBug") ?? "Contact Developer (Report Bug)";
        }

        if (BtnCancel.Content == null)
        {
            BtnCancel.Content = "Cancel";
        }
    }

    private void BtnContactDeveloper_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects?.PlayNotificationSound();

        var supportRequestWindow = new SupportWindow(_exception, _contextMessage, _gameLauncher);
        supportRequestWindow.Show();

        Close();
    }

    private void BtnAskPerplexity_Click(object sender, RoutedEventArgs e)
    {
        LaunchAiSearch(_configuration["Urls:PerplexitySearch"] ?? "https://www.perplexity.ai/search?q=");
    }

    private void BtnAskPhind_Click(object sender, RoutedEventArgs e)
    {
        LaunchAiSearch(_configuration["Urls:PhindSearch"] ?? "https://www.phind.com/search?q=");
    }

    private void BtnAskYou_Click(object sender, RoutedEventArgs e)
    {
        LaunchAiSearch(_configuration["Urls:YouSearch"] ?? "https://you.com/search?q=");
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
            MessageBoxLibrary.CouldNotOpenBrowserForAiSupport();

            var contextMessage = $"Error in LaunchAiSearch with base URL: {baseUrl}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }

        Close();
    }

    private string BuildQuery()
    {
        var sb = new StringBuilder();
        sb.Append("I am a user of the emulator frontend 'Simple Launcher'. It launch emulators via CLI params.");
        sb.Append("I am having trouble launching my game. Please help me out.");
        sb.Append("I do not know if I choose the right core.");
        sb.Append("Maybe the paths are incorrect.");
        sb.Append("Provide me a very simple explanation of the problem and help me fix the parameters.");

        // Retrieve the URL from appsettings.json
        var wikiParametersUrl = _configuration.GetValue<string>("WikiParametersUrl");
        if (!string.IsNullOrEmpty(wikiParametersUrl))
        {
            sb.Append(CultureInfo.InvariantCulture, $"'Simple Launcher' parameters reference can be found on {wikiParametersUrl}.");
        }
        else
        {
            // Fallback to hardcoded URL if not found in config, or log an error
            sb.Append(" 'Simple Launcher' parameters reference can be found on https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters.");
            // Log this fallback as a warning if ILogErrors was available here
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
                // Truncate stack trace to avoid extremely long URLs
                var stackTrace = _exception.StackTrace.Length > 1500
                    ? string.Concat(_exception.StackTrace.AsSpan(0, 1500), "...")
                    : _exception.StackTrace;
                sb.Append(CultureInfo.InvariantCulture, $"Stack Trace: {stackTrace}");
            }
        }

        return sb.ToString();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}