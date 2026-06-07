using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.GetApplicationVersion;
using SimpleLauncher.Services.MessageBox;
using UpdateChecker = SimpleLauncher.Services.CheckForUpdates.UpdateChecker;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the AboutWindow.
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private string _appVersion;

    public AboutViewModel(ILogErrors logErrors)
    {
        _logErrors = logErrors;
        AppVersion = GetApplicationVersion.GetVersion;
    }

    /// <summary>
    /// Gets the application version string.
    /// </summary>
    public string AppVersion
    {
        get => _appVersion;
        private set => SetProperty(ref _appVersion, value);
    }

    private bool _isCheckingForUpdates;

    /// <summary>
    /// Gets whether an update check is in progress.
    /// </summary>
    public bool IsCheckingForUpdates
    {
        get => _isCheckingForUpdates;
        private set
        {
            if (SetProperty(ref _isCheckingForUpdates, value))
            {
                CheckForUpdatesCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Event raised when the window should be closed.
    /// </summary>
    public event Action CloseRequested;

    /// <summary>
    /// Event raised when the update history window should be opened.
    /// </summary>
    public event Action OpenUpdateHistoryRequested;

    /// <summary>
    /// Event raised to request the owner window for dialogs.
    /// </summary>
    public event Func<Window> GetOwnerWindow;

    private bool CanCheckForUpdates => !IsCheckingForUpdates;

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanCheckForUpdates))]
    private async Task CheckForUpdatesAsync()
    {
        IsCheckingForUpdates = true;

        try
        {
            var updateChecker = App.ServiceProvider.GetRequiredService<UpdateChecker>();
            var ownerWindow = GetOwnerWindow?.Invoke();
            await updateChecker.ManualCheckForUpdatesAsync(ownerWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the CheckForUpdateAsync_Click method.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox();
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    [RelayCommand]
    private void OpenUpdateHistory()
    {
        OpenUpdateHistoryRequested?.Invoke();
    }

    [RelayCommand]
    private void OpenWebsite(string url)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the Hyperlink_RequestNavigate method.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }
    }
}
