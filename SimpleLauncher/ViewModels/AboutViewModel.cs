using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.GetApplicationVersion;
using UpdateChecker = SimpleLauncher.Services.CheckForUpdates.UpdateChecker;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the AboutWindow.
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly UpdateChecker _updateChecker;
    private string _appVersion;

    public AboutViewModel(ILogErrors logErrors, IMessageBoxLibraryService messageBox, UpdateChecker updateChecker)
    {
        _logErrors = logErrors;
        _messageBox = messageBox;
        _updateChecker = updateChecker;
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
            var ownerWindow = GetOwnerWindow?.Invoke();
            await _updateChecker.ManualCheckForUpdatesAsync(ownerWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the CheckForUpdateAsync_Click method.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBox.ErrorCheckingForUpdatesMessageBox();
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
    private async Task OpenWebsiteAsync(string url)
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
            await _messageBox.UnableToOpenLinkMessageBox();
        }
    }
}
