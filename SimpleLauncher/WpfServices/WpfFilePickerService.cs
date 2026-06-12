#nullable enable

using Microsoft.Win32;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

/// <summary>
/// WPF implementation of IFilePickerService, providing file open, folder open, and save file dialogs.
/// </summary>
public class WpfFilePickerService : IFilePickerService
{
    // WPF dialog APIs (ShowDialog) are inherently synchronous on the UI thread.
    // Task.FromResult is used to satisfy the async interface contract without
    // introducing unnecessary state machines. This is the standard .NET pattern
    // for wrapping synchronous results behind an async interface.

    /// <summary>Displays an open file dialog and returns the selected file path, or null if cancelled.</summary>
    public Task<string?> OpenFileAsync(string title, string filter = "All files|*.*")
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }

    /// <summary>Displays an open folder dialog and returns the selected folder path, or null if cancelled.</summary>
    public Task<string?> OpenFolderAsync(string title)
    {
        var dialog = new OpenFolderDialog
        {
            Title = title
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FolderName : null);
    }

    /// <summary>Displays a save file dialog and returns the selected file path, or null if cancelled.</summary>
    public Task<string?> SaveFileAsync(string title, string filter = "All files|*.*")
    {
        var dialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }
}
