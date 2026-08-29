using System.Windows;
using Microsoft.Win32;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher.InjectConfigWindows;

/// <summary>
///     Window for injecting Flycast emulator configuration settings.
/// </summary>
public partial class InjectFlycastConfigWindow
{
    private readonly Func<Window> _getOwnerWindowHandler;
    private readonly Func<string?> _requestEmulatorPathHandler;
    private readonly InjectFlycastConfigViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InjectFlycastConfigWindow" /> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    public InjectFlycastConfigWindow(InjectFlycastConfigViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

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

    /// <summary>
    ///     Gets whether the emulator should be launched after configuration.
    /// </summary>
    public bool ShouldRun => _viewModel.ShouldRun;

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    ///     Initializes the window with the specified emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">Optional path to the Flycast emulator executable.</param>
    /// <param name="isLauncherMode">If true, the window operates in launcher mode.</param>
    public void Initialize(string? emulatorPath = null, bool isLauncherMode = true)
    {
        _viewModel.Initialize(emulatorPath, isLauncherMode);

        if (!isLauncherMode) BtnSave.IsDefault = true;
    }

    private static string? OnRequestEmulatorPath()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Flycast Executable|flycast.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectFlycastEmulator") ?? "Select Flycast Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}