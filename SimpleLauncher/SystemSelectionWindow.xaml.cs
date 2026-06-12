using SystemSelectionViewModel = SimpleLauncher.ViewModels.SystemSelectionViewModel;

namespace SimpleLauncher;

/// <summary>
/// Window for selecting an emulator system from a list of available systems.
/// </summary>
public partial class SystemSelectionWindow
{
    private readonly SystemSelectionViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemSelectionWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing system selection logic.</param>
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

    /// <summary>
    /// Initializes the window with a pre-selected system guess.
    /// </summary>
    /// <param name="currentGuess">The initially selected system name.</param>
    public void Initialize(string currentGuess)
    {
        _viewModel.Initialize(currentGuess);
    }

    /// <summary>
    /// Gets the system name selected by the user.
    /// </summary>
    public string SelectedSystem => _viewModel.SelectedSystem;
}
