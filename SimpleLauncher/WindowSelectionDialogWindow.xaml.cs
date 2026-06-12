using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Dialog window for selecting a running application window by handle.
/// </summary>
public partial class WindowSelectionDialogWindow
{
    private readonly WindowSelectionDialogViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowSelectionDialogWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing window selection logic.</param>
    public WindowSelectionDialogWindow(WindowSelectionDialogViewModel viewModel)
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

        // Set default DialogResult to false
        Closed += (_, _) => { DialogResult ??= false; };
    }

    /// <summary>
    /// Initializes the window with a list of available application windows.
    /// </summary>
    /// <param name="windows">Collection of window handles and titles to display.</param>
    public void Initialize(IEnumerable<(IntPtr Handle, string Title)> windows)
    {
        _viewModel.Initialize(windows);
    }

    /// <summary>
    /// Gets the handle of the window selected by the user.
    /// </summary>
    public IntPtr SelectedWindowHandle => _viewModel.SelectedWindowHandle;
}
