#nullable enable
namespace SimpleLauncher.Core.Interfaces;

public interface IFilePickerService
{
    Task<string?> OpenFileAsync(string title, string filter = "All files|*.*");
    Task<string?> OpenFolderAsync(string title);
    Task<string?> SaveFileAsync(string title, string filter = "All files|*.*");
}
