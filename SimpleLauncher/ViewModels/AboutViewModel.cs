using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GetApplicationVersion;
using SimpleLauncher.Services.MessageBox;
using UpdateChecker = SimpleLauncher.Services.CheckForUpdates.UpdateChecker;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the AboutWindow.
/// </summary>
public class AboutViewModel : ViewModelBase
{
    private string _appVersion;

    public AboutViewModel()
    {
        AppVersion = GetApplicationVersion.GetVersion;

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        CheckForUpdatesCommand = new RelayCommand(_ => CheckForUpdatesAsync().ConfigureAwait(false), _ => !IsCheckingForUpdates);
        OpenUpdateHistoryCommand = new RelayCommand(_ => OpenUpdateHistoryRequested?.Invoke());
        OpenWebsiteCommand = new RelayCommand<string>(OpenWebsite);
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
                // Re-evaluate CanExecute for CheckForUpdatesCommand
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    /// <summary>
    /// Command to close the window.
    /// </summary>
    public ICommand CloseCommand { get; }

    /// <summary>
    /// Command to check for updates.
    /// </summary>
    public ICommand CheckForUpdatesCommand { get; }

    /// <summary>
    /// Command to open the update history window.
    /// </summary>
    public ICommand OpenUpdateHistoryCommand { get; }

    /// <summary>
    /// Command to open a website URL.
    /// </summary>
    public ICommand OpenWebsiteCommand { get; }

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox();
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }
    }
}
