using Microsoft.Win32;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.New.Services.WpfServices;

/// <summary>
/// WPF implementation of IFilePickerService — provides async file/folder dialogs.
/// </summary>
public class WpfFilePickerService : IFilePickerService
{
    public Task<string?> OpenFileAsync(string title, string filter = "All files|*.*")
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter,
            CheckFileExists = true
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileName : null);
    }

    public Task<string?> OpenFolderAsync(string title)
    {
        var dialog = new OpenFolderDialog
        {
            Title = title
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FolderName : null);
    }

    public Task<string?> SaveFileAsync(string title, string filter = "All files|*.*")
    {
        var dialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileName : null);
    }
}
