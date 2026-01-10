using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleLauncher.Services;

public class SupportFromTheDeveloper
{
    internal static void DoYouWantToReceiveSupportFromTheDeveloper(Exception ex = null, string contextMessage = null, GameLauncher gameLauncher = null, PlaySoundEffects playSoundEffects = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var configuration = App.ServiceProvider.GetRequiredService<IConfiguration>();
            var supportOptionWindow = new SupportOptionWindow(ex, contextMessage, gameLauncher, playSoundEffects, configuration);
            // Show it as a dialog (modal) so it blocks interaction with the main window until a choice is made
            supportOptionWindow.ShowDialog();
        });
    }
}