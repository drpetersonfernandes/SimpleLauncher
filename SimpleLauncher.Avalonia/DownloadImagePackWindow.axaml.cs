using Avalonia.Controls;
using Avalonia.Interactivity;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Window that downloads and installs image packs for game systems.
/// </summary>
public partial class DownloadImagePackWindow : Window, IDisposable
{
    private readonly DownloadImagePackViewModel _viewModel;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadImagePackWindow"/> class.
    /// </summary>
    /// <param name="logErrors">The logger instance for error logging.</param>
    /// <param name="viewModel">The view model providing download and extraction logic.</param>
    public DownloadImagePackWindow(ILogger logErrors, DownloadImagePackViewModel viewModel,
        Services.LocalizationService localization)
    {
        InitializeComponent();
        _logger = logErrors;

        _viewModel = viewModel;
        DataContext = _viewModel;

        // Localize the emergency return button (WPF DynamicResource ReturnButton parity)
        EmergencyButton.Content = localization.GetString("ReturnButton");
        ToolTip.SetTip(EmergencyButton,
            localization.GetString("ClickHereIfTheLoadingScreenIsStuckToReturnToTheMainMenu"));

        Closing += CloseWindowRoutineAsync;
        Loaded += DownloadImagePackWindowLoadedAsync;
    }

    private async void DownloadImagePackWindowLoadedAsync(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[DownloadImagePackWindowLoadedAsync] Error initializing EasyModeManager.");
        }
    }

    private async void CloseWindowRoutineAsync(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            await _viewModel.CloseWindowRoutineAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in method CloseWindowRoutineAsync.");
        }
        finally
        {
            Dispose();
        }
    }

    private void EmergencyOverlayRelease_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.EmergencyOverlayRelease();
    }

    /// <summary>
    /// Disposes of resources used by the window.
    /// </summary>
    public void Dispose()
    {
        _viewModel.Dispose();
        GC.SuppressFinalize(this);
    }
}