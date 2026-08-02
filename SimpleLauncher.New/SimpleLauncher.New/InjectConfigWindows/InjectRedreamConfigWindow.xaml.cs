using System.Windows;
using SimpleLauncher.New.ViewModels;

namespace SimpleLauncher.New.InjectConfigWindows;

/// <summary>
/// Window for injecting Redream emulator configuration settings.
/// </summary>
public partial class InjectRedreamConfigWindow
{
    private readonly InjectRedreamConfigViewModel _viewModel;
    private readonly Func<string?> _requestEmulatorPathHandler;
    private readonly Func<Window> _getOwnerWindowHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectRedreamConfigWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    public InjectRedreamConfigWindow(InjectRedreamConfigViewModel viewModel)
    {
        InitializeComponent();


        _viewModel = viewModel;
        _requestEmulatorPathHandler = OnRequestEmulatorPath;
        _getOwnerWindowHandler = () => this;

        _viewModel.CloseRequested += OnCloseRequested;
        _viewModel.RequestEmulatorPath += _requestEmulatorPathHandler;
        _viewModel.GetOwnerWindow += _getOwnerWindowHandler;

        Closing += (_, _) =>
        {
            _viewModel.CloseRequested -= OnCloseRequested;
            _viewModel.RequestEmulatorPath -= _requestEmulatorPathHandler;
            _viewModel.GetOwnerWindow -= _getOwnerWindowHandler;
        };

        DataContext = _viewModel;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Initializes the window with the specified emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">Optional path to the Redream emulator executable.</param>
    /// <param name="isLauncherMode">If true, the window operates in launcher mode.</param>
    public void Initialize(string? emulatorPath = null, bool isLauncherMode = true)
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

    private static string? OnRequestEmulatorPath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Redream Executable|redream.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectRedreamEmulator") ?? "Select Redream Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
