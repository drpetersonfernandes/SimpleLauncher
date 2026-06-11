using SystemSelectionViewModel = SimpleLauncher.ViewModels.SystemSelectionViewModel;

namespace SimpleLauncher;

public partial class SystemSelectionWindow
{
    private readonly SystemSelectionViewModel _viewModel;

    public SystemSelectionWindow(SystemSelectionViewModel viewModel)
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
    }

    public void Initialize(string currentGuess)
    {
        _viewModel.Initialize(currentGuess);
    }

    public string SelectedSystem => _viewModel.SelectedSystem;
}