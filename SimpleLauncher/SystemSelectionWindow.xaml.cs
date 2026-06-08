using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher;

public partial class SystemSelectionWindow
{
    private readonly SystemSelectionViewModel _viewModel;

    public SystemSelectionWindow(SystemSelectionViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.DialogResultRequested += result =>
        {
            DialogResult = result;
            Close();
        };

        DataContext = _viewModel;
    }

    public void Initialize(string currentGuess)
    {
        _viewModel.Initialize(currentGuess);
    }

    public string SelectedSystem => _viewModel.SelectedSystem;
}