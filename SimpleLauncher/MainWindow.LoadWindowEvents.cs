using System;
using System.Windows;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded += async (_, _) =>
        {
            try
            {
                await DisplaySystemSelectionScreenAsync();
                await UpdateChecker.SilentCheckForUpdatesAsync(this);
                await Stats.CallApiAsync();
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, "Error in the Loaded event.");
                DebugLogger.Log($"Error in the Loaded event: {ex.Message}");
            }
        };

        // Apply language
        SetLanguageAndCheckMenu(_settings.Language);

        // Apply Theme
        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);

        // Load previous windows state
        Width = _settings.MainWindowWidth;
        Height = _settings.MainWindowHeight;
        Top = _settings.MainWindowTop;
        Left = _settings.MainWindowLeft;

        if (!Enum.TryParse<WindowState>(_settings.MainWindowState, out var windowState))
        {
            windowState = WindowState.Normal;
        }

        WindowState = windowState;

        // Set the initial SelectedSystem and PlayTime
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        SelectedSystem = nosystemselected;
        PlayTime = "00:00:00";

        // Set the initial ViewMode based on the _settings
        SetViewMode(_settings.ViewMode);

        // Check if the application has write access
        if (!CheckIfDirectoryIsWritable.IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory))
        {
            MessageBoxLibrary.MoveToWritableFolderMessageBox();
        }

        // Set initial pagination state
        PrevPageButton.IsEnabled = false;
        NextPageButton.IsEnabled = false;
        _prevPageButton = PrevPageButton;
        _nextPageButton = NextPageButton;

        // Initialize TrayIconManager
        _trayIconManager = new TrayIconManager(this, _settings);

        // Check for required files
        CheckForRequiredFiles.CheckFiles();

        // --- Set initial checked state for overlay buttons ---
        RetroAchievementButton.IsChecked = _settings.OverlayRetroAchievementButton;
        VideoLinkButton.IsChecked = _settings.OverlayOpenVideoButton;
        InfoLinkButton.IsChecked = _settings.OverlayOpenInfoButton;

        // Initialize the GamePadController
        GamePadController.Instance2.ErrorLogger = (ex, msg) => { _ = LogErrors.LogErrorAsync(ex, msg); };
        if (_settings.EnableGamePadNavigation)
        {
            GamePadController.Instance2.Start();
        }
        else
        {
            GamePadController.Instance2.Stop();
        }

        // Update the GamePadController dead zone settings from SettingsManager
        GamePadController.Instance2.DeadZoneX = _settings.DeadZoneX;
        GamePadController.Instance2.DeadZoneY = _settings.DeadZoneY;

        InitializeControllerDetection();
    }
}