using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

using Interfaces;

/// <summary>
/// Window for submitting support requests and bug reports.
/// </summary>
public partial class SupportWindow : ILoadingState
{
    private readonly SupportViewModel _viewModel;
    private readonly IDebugLogger _debugLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupportWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing support form logic.</param>
    /// <param name="debugLogger">The debug logger.</param>
    public SupportWindow(SupportViewModel viewModel, IDebugLogger debugLogger)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _viewModel = viewModel;

        _viewModel.CloseRequested += Close;
        _viewModel.FormCleared += () =>
        {
            NameTextBox.Text = "";
            EmailTextBox.Text = "";
            SupportTextBox.Text = "";
        };

        Loaded += (_, _) =>
        {
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }
        };

        DataContext = _viewModel;
    }

    /// <summary>
    /// Toggles the loading overlay with an optional message.
    /// </summary>
    /// <param name="isLoading">Whether to show or hide the loading overlay.</param>
    /// <param name="message">Optional message to display while loading.</param>
    public void SetLoadingState(bool isLoading, string message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            MainContentGrid?.IsEnabled = !isLoading;

            if (isLoading)
            {
                LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        MainContentGrid?.IsEnabled = true;

        _debugLogger.Log("[Emergency] User forced overlay dismissal in SupportWindow.");
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }
}
