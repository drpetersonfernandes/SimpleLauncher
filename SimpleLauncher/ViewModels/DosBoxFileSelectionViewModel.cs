using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Models;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the DosBoxFileSelectionWindow.
/// </summary>
public partial class DosBoxFileSelectionViewModel : ObservableObject
{
    private DosBoxFileItem _selectedItem;
    private bool _isLaunchEnabled;

    public void Initialize(List<string> filePaths, string baseDirectory)
    {
        var fileItems = filePaths.Select(path => new DosBoxFileItem
        {
            FullPath = path,
            DisplayName = Path.GetFileName(path),
            RelativePath = GetRelativePath(path, baseDirectory)
        }).ToList();

        FileItems = new ObservableCollection<DosBoxFileItem>(fileItems);
        OnPropertyChanged(nameof(FileItems));
    }

    /// <summary>
    /// Gets the collection of file items.
    /// </summary>
    public ObservableCollection<DosBoxFileItem> FileItems { get; private set; } = [];

    /// <summary>
    /// Gets or sets the selected file item.
    /// </summary>
    public DosBoxFileItem SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                IsLaunchEnabled = value != null;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the launch button is enabled.
    /// </summary>
    public bool IsLaunchEnabled
    {
        get => _isLaunchEnabled;
        private set => SetProperty(ref _isLaunchEnabled, value);
    }

    /// <summary>
    /// Gets the selected file path after dialog closes.
    /// </summary>
    public string SelectedFilePath { get; private set; }

    /// <summary>
    /// Event raised when the window should be closed with a dialog result.
    /// </summary>
    public event Action<bool?> DialogResultRequested;

    [RelayCommand]
    private void Launch()
    {
        if (SelectedItem == null) return;

        SelectedFilePath = SelectedItem.FullPath;
        DialogResultRequested?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResultRequested?.Invoke(false);
    }

    /// <summary>
    /// Handles double-click on an item.
    /// </summary>
    public void OnItemDoubleClicked()
    {
        if (SelectedItem == null) return;

        SelectedFilePath = SelectedItem.FullPath;
        DialogResultRequested?.Invoke(true);
    }

    private static string GetRelativePath(string fullPath, string baseDirectory)
    {
        var dir = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(dir) || dir.Equals(baseDirectory, StringComparison.OrdinalIgnoreCase))
            return "";

        var relative = dir.Length > baseDirectory.Length
            ? dir[(baseDirectory.Length + 1)..]
            : dir;

        return relative;
    }
}