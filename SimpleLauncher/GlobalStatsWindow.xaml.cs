using System.ComponentModel;
using System.Windows;
using SimpleLauncher.ViewModels;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

internal partial class GlobalStatsWindow : IDisposable
{
    private readonly GlobalStatsViewModel _viewModel;
    private Action _closeRequestedHandler;

    internal GlobalStatsWindow(GlobalStatsViewModel viewModel)
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
    }

    internal void Initialize(List<SystemManager> systemManagers)
    {
        _viewModel.Initialize(systemManagers);
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
        _viewModel.Dispose();
    }
}
