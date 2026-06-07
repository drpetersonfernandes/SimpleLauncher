using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher;

public partial class DosBoxFileSelectionWindow
{
    private readonly DosBoxFileSelectionViewModel _viewModel;

    public DosBoxFileSelectionWindow(DosBoxFileSelectionViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _viewModel.DialogResultRequested += result =>
        {
            DialogResult = result;
            Close();
        };

        DataContext = _viewModel;

        Closed += (_, _) => { DialogResult ??= false; };
    }

    public void Initialize(List<string> filePaths, string baseDirectory)
    {
        _viewModel.Initialize(filePaths, baseDirectory);
    }

    public string SelectedFilePath => _viewModel.SelectedFilePath;

    private void FileListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _viewModel.OnItemDoubleClicked();
    }
}