using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class SystemSelectionWindow
{
    private readonly SystemSelectionViewModel _viewModel;

    public SystemSelectionWindow(string currentGuess)
    {
        InitializeComponent();

        _viewModel = new SystemSelectionViewModel(currentGuess);
        _viewModel.DialogResultRequested += result =>
        {
            DialogResult = result;
            Close();
        };

        DataContext = _viewModel;
    }

    public string SelectedSystem => _viewModel.SelectedSystem;
}