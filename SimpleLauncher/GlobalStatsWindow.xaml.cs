using System.ComponentModel;
using System.Windows;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.ViewModels;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

internal partial class GlobalStatsWindow : IDisposable
{
    private readonly GlobalStatsViewModel _viewModel;

    internal GlobalStatsWindow(GlobalStatsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.CloseRequested += () =>
        {
            Application.Current.Dispatcher.InvokeAsync(Close);
        };
        _viewModel.ConfirmSaveReportRequested += MessageBoxLibrary.WoulYouLikeToSaveAReportMessageBox;
        _viewModel.ConfirmCancelRequested += MessageBoxLibrary.DoYouWantToCancelAndCloseMessageBox;

        DataContext = _viewModel;
        App.ApplyThemeToWindow(this);
    }

    internal void Initialize(List<SystemManager> systemManagers)
    {
        _viewModel.Initialize(systemManagers);
    }

    private void GlobalStatsWindow_Closing(object sender, CancelEventArgs e)
    {
        // Execute the closing command
        if (_viewModel.ClosingCommand.CanExecute(e))
        {
            _viewModel.ClosingCommand.Execute(e);
        }
    }

    public void Dispose()
    {
        _viewModel?.Dispose();
    }
}
