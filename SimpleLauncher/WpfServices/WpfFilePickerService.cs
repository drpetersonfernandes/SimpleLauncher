#nullable enable

using Microsoft.Win32;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfFilePickerService : IFilePickerService
{
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
