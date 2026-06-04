using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class DosBoxFileSelectionWindow
{
    private readonly DosBoxFileSelectionViewModel _viewModel;

    public DosBoxFileSelectionWindow(List<string> filePaths, string baseDirectory)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new DosBoxFileSelectionViewModel(filePaths, baseDirectory);
        _viewModel.DialogResultRequested += result =>
        {
            DialogResult = result;
            Close();
        };

        DataContext = _viewModel;

        Closed += (_, _) => { DialogResult ??= false; };
    }

    public string SelectedFilePath => _viewModel.SelectedFilePath;

    private void FileListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _viewModel.OnItemDoubleClicked();
    }
}