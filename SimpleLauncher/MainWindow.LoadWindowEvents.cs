using System;
using System.Windows;
using System.Windows.Threading;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using Microsoft.Extensions.Configuration;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply language
        SetLanguageAndCheckMenu(_settings.Language);
        DebugLogger.Log("Language and menu was set.");

        // Apply Theme
        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
        DebugLogger.Log("Theme was set.");

        // Set the initial SelectedSystem and PlayTime
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        SelectedSystem = nosystemselected;
        PlayTime = "00:00:00";
        DebugLogger.Log("SelectedSystem and PlayTime was set.");

        // Set the initial ViewMode based on the _settings
        SetViewMode(_settings.ViewMode);
        DebugLogger.Log("ViewMode was set.");

        // Check if the application has write access
        if (!CheckIfDirectoryIsWritable.IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory))
        {
            MessageBoxLibrary.MoveToWritableFolderMessageBox();
            DebugLogger.Log("Application does not have write access.");
        }

        // Set initial pagination state
        PrevPageButton.IsEnabled = false;
        NextPageButton.IsEnabled = false;
        _prevPageButton = PrevPageButton;
        _nextPageButton = NextPageButton;
        DebugLogger.Log("Pagination was set.");

        // Initialize TrayIconManager
        _trayIconManager = new TrayIconManager(this, _settings, _gamePadController);
        DebugLogger.Log("TrayIconManager was initialized.");

        // Check for required files
        CheckForRequiredFiles.CheckFiles();
        DebugLogger.Log("Required files were checked.");

        // --- Set initial checked state for overlay buttons ---
        RetroAchievementButton.IsChecked = _settings.OverlayRetroAchievementButton;
        VideoLinkButton.IsChecked = _settings.OverlayOpenVideoButton;
        InfoLinkButton.IsChecked = _settings.OverlayOpenInfoButton;
        DebugLogger.Log("Overlay buttons were set.");

        // Initialize the GamePadController
        _gamePadController.ErrorLogger = (ex, msg) => { _ = _logErrors.LogErrorAsync(ex, msg); };
        if (_settings.EnableGamePadNavigation)
        {
            _gamePadController.Start();
        }
        else
        {
            _gamePadController.Stop();
        }

        DebugLogger.Log("GamePadController was initialized.");

        // Update the GamePadController dead zone settings from SettingsManager
        _gamePadController.DeadZoneX = _settings.DeadZoneX;
        _gamePadController.DeadZoneY = _settings.DeadZoneY;
        DebugLogger.Log("GamePadController dead zone settings were updated.");

        // Initialize the status bar timer
        StatusBarTimer = new DispatcherTimer();

        var statusBarTimeoutSeconds = App.Configuration.GetValue("StatusBarTimeoutSeconds", 3);
        StatusBarTimer.Interval = TimeSpan.FromSeconds(statusBarTimeoutSeconds);

        StatusBarTimer.Tick += (s, eventArgs) =>
        {
            StatusBarText.Content = ""; // Clear the status bar
            StatusBarTimer.Stop(); // Stop the timer after clearing
        };
    }
}