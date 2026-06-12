using System.Windows;
using DosBoxFileSelectionViewModel = SimpleLauncher.ViewModels.DosBoxFileSelectionViewModel;

namespace SimpleLauncher;

/// <summary>
/// Window for selecting a DOSBox file from a list of available files.
/// </summary>
public partial class DosBoxFileSelectionWindow
{
    private readonly DosBoxFileSelectionViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="DosBoxFileSelectionWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing file selection logic.</param>
    public DosBoxFileSelectionWindow(DosBoxFileSelectionViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _viewModel = viewModel;
        _viewModel.DialogResultRequested += result =>
        {
            if (IsLoaded)
            {
                DialogResult = result;
            }

            Close();
        };

        DataContext = _viewModel;

        Closed += (_, _) => { DialogResult ??= false; };
    }

    /// <summary>
    /// Initializes the window with the specified file paths and base directory.
    /// </summary>
    /// <param name="filePaths">The list of file paths to display.</param>
    /// <param name="baseDirectory">The base directory for resolving relative paths.</param>
    public void Initialize(List<string> filePaths, string baseDirectory)
    {
        _viewModel.Initialize(filePaths, baseDirectory);
    }

    /// <summary>
    /// Gets the file path selected by the user.
    /// </summary>
    public string SelectedFilePath => _viewModel.SelectedFilePath;

    private void FileListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _viewModel.OnItemDoubleClicked();
    }
}
