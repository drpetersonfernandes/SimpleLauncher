using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for injecting Mesen emulator configuration settings.
/// </summary>
public partial class InjectMesenConfigWindow
{
    private readonly InjectMesenConfigViewModel _viewModel;
    private readonly Func<string> _requestEmulatorPathHandler;
    private readonly Func<Window> _getOwnerWindowHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectMesenConfigWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    public InjectMesenConfigWindow(InjectMesenConfigViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _requestEmulatorPathHandler = OnRequestEmulatorPath;
        _getOwnerWindowHandler = () => this;

        _viewModel.CloseRequested += Close;
        _viewModel.RequestEmulatorPath += _requestEmulatorPathHandler;
        _viewModel.GetOwnerWindow += _getOwnerWindowHandler;

        Closing += (_, _) =>
        {
            _viewModel.CloseRequested -= Close;
            _viewModel.RequestEmulatorPath -= _requestEmulatorPathHandler;
            _viewModel.GetOwnerWindow -= _getOwnerWindowHandler;
        };

        DataContext = _viewModel;
    }

    /// <summary>
    /// Initializes the window with the specified emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">Optional path to the Mesen emulator executable.</param>
    /// <param name="isLauncherMode">If true, the window operates in launcher mode.</param>
    public void Initialize(string emulatorPath = null, bool isLauncherMode = true)
    {
        _viewModel.Initialize(emulatorPath, isLauncherMode);

        if (!isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    /// <summary>
    /// Gets whether the emulator should be launched after configuration.
    /// </summary>
    public bool ShouldRun => _viewModel.ShouldRun;

    private static string OnRequestEmulatorPath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Mesen Executable|Mesen.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectMesenEmulator") ?? "Select Mesen Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
