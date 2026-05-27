using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

internal partial class DownloadImagePackWindow
{
    private readonly DownloadImagePackViewModel _viewModel;

    internal DownloadImagePackWindow(PlaySoundEffects playSoundEffects)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new DownloadImagePackViewModel(playSoundEffects);
        DataContext = _viewModel;

        Closed += CloseWindowRoutineAsync;
        Loaded += DownloadImagePackWindowLoadedAsync;

        Loaded += (_, _) =>
        {
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }
        };
    }

    private async void DownloadImagePackWindowLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            App.LogErrorAsync(ex, "[DownloadImagePackWindowLoadedAsync] Error initializing EasyModeManager.");
        }
    }

    private async void CloseWindowRoutineAsync(object sender, EventArgs e)
    {
        try
        {
            await _viewModel.CloseWindowRoutineAsync();
        }
        catch (Exception ex)
        {
            App.LogErrorAsync(ex, "Error in method CloseWindowRoutineAsync.");
        }
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.EmergencyOverlayRelease();
    }
}
