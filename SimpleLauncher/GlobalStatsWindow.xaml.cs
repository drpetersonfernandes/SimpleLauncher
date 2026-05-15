using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.ViewModels;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

internal partial class GlobalStatsWindow : IDisposable
{
    private readonly GlobalStatsViewModel _viewModel;

    internal GlobalStatsWindow(List<SystemManager> systemManagers, IConfiguration configuration)
    {
        InitializeComponent();

        _viewModel = new GlobalStatsViewModel(systemManagers, configuration);
        _viewModel.CloseRequested += () =>
        {
            Application.Current.Dispatcher.InvokeAsync(Close);
        };
        _viewModel.ConfirmSaveReportRequested += MessageBoxLibrary.WoulYouLikeToSaveAReportMessageBox;
        _viewModel.ConfirmCancelRequested += MessageBoxLibrary.DoYouWantToCancelAndCloseMessageBox;

        DataContext = _viewModel;
        App.ApplyThemeToWindow(this);
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
