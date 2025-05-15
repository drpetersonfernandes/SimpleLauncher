using System;
using System.Windows;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
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

        // Update the GamePadController dead zone settings from SettingsManager
        GamePadController.Instance2.DeadZoneX = _settings.DeadZoneX;
        GamePadController.Instance2.DeadZoneY = _settings.DeadZoneY;

        InitializeControllerDetection();

        // Initialize TrayIconManager
        _trayIconManager = new TrayIconManager(this, _settings);

        // Initialize PlayHistory
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();

        // Check for required files
        CheckForRequiredFiles.CheckFiles();
    }
}