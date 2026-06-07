using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher;

public partial class WindowSelectionDialogWindow
{
    private readonly WindowSelectionDialogViewModel _viewModel;

    public WindowSelectionDialogWindow(WindowSelectionDialogViewModel viewModel)
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

        // Set default DialogResult to false
        Closed += (_, _) => { DialogResult ??= false; };
    }

    public void Initialize(IEnumerable<(IntPtr Handle, string Title)> windows)
    {
        _viewModel.Initialize(windows);
    }

    public IntPtr SelectedWindowHandle => _viewModel.SelectedWindowHandle;
}