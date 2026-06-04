using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class WindowSelectionDialogWindow
{
    private readonly WindowSelectionDialogViewModel _viewModel;

    public WindowSelectionDialogWindow(IEnumerable<(IntPtr Handle, string Title)> windows)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new WindowSelectionDialogViewModel(windows);
        _viewModel.DialogResultRequested += result =>
        {
            DialogResult = result;
            Close();
        };

        DataContext = _viewModel;

        // Set default DialogResult to false
        Closed += (_, _) => { DialogResult ??= false; };
    }

    public IntPtr SelectedWindowHandle => _viewModel.SelectedWindowHandle;
}