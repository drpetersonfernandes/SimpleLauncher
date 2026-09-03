using System.Windows;
using System.Windows.Input;
using DosBoxFileSelectionViewModel = SimpleLauncher.ViewModels.DosBoxFileSelectionViewModel;

namespace SimpleLauncher;

/// <summary>
///     Window for selecting a DOSBox file from a list of available files.
/// </summary>
public partial class DosBoxFileSelectionWindow
{
    private readonly DosBoxFileSelectionViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DosBoxFileSelectionWindow" /> class.
    /// </summary>
    /// <param name="viewModel">The view model providing file selection logic.</param>
    public DosBoxFileSelectionWindow(DosBoxFileSelectionViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _viewModel = viewModel;
        _viewModel.DialogResultRequested += (_, e) =>
        {
            if (IsLoaded) DialogResult = e.Value;

            Close();
        };

        DataContext = _viewModel;

        Closed += (_, _) => DialogResult ??= false;
    }

    /// <summary>
    ///     Gets the file path selected by the user.
    /// </summary>
    public string SelectedFilePath => _viewModel.SelectedFilePath;

    /// <summary>
    ///     Initializes the window with the specified file paths and base directory.
    /// </summary>
    /// <param name="filePaths">The list of file paths to display.</param>
    /// <param name="baseDirectory">The base directory for resolving relative paths.</param>
    public void Initialize(IList<string> filePaths, string baseDirectory)
    {
        _viewModel.Initialize(filePaths, baseDirectory);
    }

    private void FileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.OnItemDoubleClicked();
    }
}