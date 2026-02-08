using System;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.GameLauncher;

public class SupportFromTheDeveloper
{
    internal static void DoYouWantToReceiveSupportFromTheDeveloper(IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogErrors logErrors,
        Exception ex = null,
        string contextMessage = null,
        PlaySoundEffects playSoundEffects = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var supportOptionWindow = new SupportOptionWindow(ex, contextMessage, playSoundEffects, configuration, httpClientFactory, logErrors);
            // Show it as a dialog (modal) so it blocks interaction with the main window until a choice is made
            supportOptionWindow.ShowDialog();
        });
    }
}