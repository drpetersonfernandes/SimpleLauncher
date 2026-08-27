using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Window for configuring RetroAchievements credentials and settings.
/// </summary>
public partial class RetroAchievementsSettingsWindow : Window
{
    private readonly RetroAchievementsSettingsViewModel _viewModel;
    private readonly EventHandler _saveCompletedHandler;
    private readonly EventHandler _closeRequestedHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetroAchievementsSettingsWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing settings logic.</param>
    public RetroAchievementsSettingsWindow(RetroAchievementsSettingsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        _saveCompletedHandler = (_, _) => { Close(); };

        _closeRequestedHandler = (_, _) => { Close(); };

        _viewModel.SaveCompleted += _saveCompletedHandler;
        _viewModel.CloseRequested += _closeRequestedHandler;
        _viewModel.RequestExePath += OnRequestExePath;

        Closing += (_, _) =>
        {
            _viewModel.SaveCompleted -= _saveCompletedHandler;
            _viewModel.CloseRequested -= _closeRequestedHandler;
            _viewModel.RequestExePath -= OnRequestExePath;
        };

        ApiKeyPasswordBox.TextChanged += (_, _) => { _viewModel.ApiKey = ApiKeyPasswordBox.Text ?? ""; };
        RaPasswordPasswordBox.TextChanged += (_, _) => { _viewModel.Password = RaPasswordPasswordBox.Text ?? ""; };

        Opened += (_, _) =>
        {
            ApiKeyPasswordBox.Text = _viewModel.ApiKey;
            RaPasswordPasswordBox.Text = _viewModel.Password;
        };

        DataContext = _viewModel;
    }

    private static async Task<string?> OnRequestExePath()
    {
        var filePicker = App.ServiceProvider.GetRequiredService<IFilePickerService>();
        return await filePicker.OpenFileAsync("Select Emulator Executable", "Executable files (*.exe)|*.exe");
    }

    private void OpenControlPanel_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://retroachievements.org/controlpanel.php")
                { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to open RetroAchievements control panel");
        }
    }
}