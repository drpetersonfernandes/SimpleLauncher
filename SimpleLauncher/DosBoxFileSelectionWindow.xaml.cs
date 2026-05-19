using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SimpleLauncher.Models;

namespace SimpleLauncher;

public partial class DosBoxFileSelectionWindow
{
    public string SelectedFilePath { get; private set; }

    private readonly List<DosBoxFileItem> _fileItems;

    public DosBoxFileSelectionWindow(List<string> filePaths, string baseDirectory)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _fileItems = filePaths.Select(path => new DosBoxFileItem
        {
            FullPath = path,
            DisplayName = Path.GetFileName(path),
            RelativePath = GetRelativePath(path, baseDirectory)
        }).ToList();

        foreach (var item in _fileItems)
        {
            FileListBox.Items.Add(item);
        }

        Closed += (_, _) => { DialogResult ??= false; };
    }

    private static string GetRelativePath(string fullPath, string baseDirectory)
    {
        var dir = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(dir) || dir.Equals(baseDirectory, StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var relative = dir.Length > baseDirectory.Length
            ? dir[(baseDirectory.Length + 1)..]
            : dir;

        return relative;
    }

    private void FileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LaunchButton.IsEnabled = FileListBox.SelectedItem is DosBoxFileItem;
    }

    private void FileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FileListBox.SelectedItem is DosBoxFileItem item)
        {
            SelectedFilePath = item.FullPath;
            DialogResult = true;
            Close();
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (FileListBox.SelectedItem is not DosBoxFileItem item) return;

        SelectedFilePath = item.FullPath;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
