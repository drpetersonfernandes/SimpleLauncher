using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

internal partial class DownloadImagePackWindow : IDisposable
{
    private readonly DownloadImagePackViewModel _viewModel;
    private readonly ILogErrors _logErrors;

    internal DownloadImagePackWindow(PlaySoundEffects playSoundEffects, ILogErrors logErrors)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _logErrors = logErrors;

        _viewModel = new DownloadImagePackViewModel(playSoundEffects, logErrors);
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
            _logErrors.LogAndForget(ex, "[DownloadImagePackWindowLoadedAsync] Error initializing EasyModeManager.");
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
            _logErrors.LogAndForget(ex, "Error in method CloseWindowRoutineAsync.");
        }
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.EmergencyOverlayRelease();
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }
}
