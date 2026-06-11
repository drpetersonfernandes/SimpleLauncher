#nullable enable

using Microsoft.Win32;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfFilePickerService : IFilePickerService
{
    // WPF dialog APIs (ShowDialog) are inherently synchronous on the UI thread.
    // Task.FromResult is used to satisfy the async interface contract without
    // introducing unnecessary state machines. This is the standard .NET pattern
    // for wrapping synchronous results behind an async interface.

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

    public Task<string?> OpenFolderAsync(string title)
    {
        var dialog = new OpenFolderDialog
        {
            Title = title
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FolderName : null);
    }

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
