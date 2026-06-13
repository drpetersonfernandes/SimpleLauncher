using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.ViewModels;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

internal partial class GlobalStatsWindow : IDisposable
{
    private readonly GlobalStatsViewModel _viewModel;
    private Action _closeRequestedHandler;
    private Button _emergencyReturnButton;

    public GlobalStatsWindow(GlobalStatsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _closeRequestedHandler = () =>
        {
            Application.Current.Dispatcher.InvokeAsync(Close);
        };
        _viewModel.CloseRequested += _closeRequestedHandler;

        DataContext = _viewModel;
        App.ApplyThemeToWindow(this);

        Closing += GlobalStatsWindow_Closing;

        Loaded += (_, _) =>
        {
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                _emergencyReturnButton = emergencyBtn;
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }
        };
    }

    internal void Initialize(List<SystemManager> systemManagers)
    {
        _viewModel.Initialize(systemManagers);
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.EmergencyOverlayRelease();
    }

    private void GlobalStatsWindow_Closing(object sender, CancelEventArgs e)
    {
        // Unsubscribe events
        if (_closeRequestedHandler != null)
        {
            _viewModel.CloseRequested -= _closeRequestedHandler;
            _closeRequestedHandler = null;
        }

        // Execute the closing command
        if (_viewModel.ClosingCommand.CanExecute(e))
        {
            _viewModel.ClosingCommand.Execute(e);
        }

        Dispose();
    }

    public void Dispose()
    {
        if (_emergencyReturnButton != null)
        {
            _emergencyReturnButton.Click -= EmergencyOverlayRelease_Click;
            _emergencyReturnButton = null;
        }

        _viewModel.Dispose();
    }
}
