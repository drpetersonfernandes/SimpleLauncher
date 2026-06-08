using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.ViewModels;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

internal partial class GlobalStatsWindow : IDisposable
{
    private readonly GlobalStatsViewModel _viewModel;
    private readonly IMessageBoxLibraryService _messageBox;

    internal GlobalStatsWindow(GlobalStatsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();
        _viewModel.CloseRequested += () =>
        {
            Application.Current.Dispatcher.InvokeAsync(Close);
        };
        _viewModel.ConfirmSaveReportRequested += () => (System.Windows.MessageBoxResult)(int)_messageBox.WoulYouLikeToSaveAReportMessageBox().GetAwaiter().GetResult();
        _viewModel.ConfirmCancelRequested += () => (System.Windows.MessageBoxResult)(int)_messageBox.DoYouWantToCancelAndCloseMessageBox().GetAwaiter().GetResult();

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
