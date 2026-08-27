namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to open file, folder, and save file dialogs.
/// </summary>
public interface IFilePickerService
{
    /// <summary>
    /// Asynchronously opens a file selection dialog.
    /// </summary>
    /// <param name="title">The title of the dialog window.</param>
    /// <param name="filter">The file type filter string (e.g., "All files|*.*").</param>
    /// <returns>The selected file path, or null if the dialog was cancelled.</returns>
    Task<string?> OpenFileAsync(string title, string filter = "All files|*.*");

    /// <summary>
    /// Asynchronously opens a folder selection dialog.
    /// </summary>
    /// <param name="title">The title of the dialog window.</param>
    /// <returns>The selected folder path, or null if the dialog was cancelled.</returns>
    Task<string?> OpenFolderAsync(string title);

    /// <summary>
    /// Asynchronously opens a save file dialog.
    /// </summary>
    /// <param name="title">The title of the dialog window.</param>
    /// <param name="filter">The file type filter string (e.g., "All files|*.*").</param>
    /// <returns>The selected save file path, or null if the dialog was cancelled.</returns>
    Task<string?> SaveFileAsync(string title, string filter = "All files|*.*");
}