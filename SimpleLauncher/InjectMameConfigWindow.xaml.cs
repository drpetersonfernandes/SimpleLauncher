using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for injecting MAME emulator configuration settings.
/// </summary>
public partial class InjectMameConfigWindow
{
    private readonly InjectMameConfigViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectMameConfigWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    public InjectMameConfigWindow(InjectMameConfigViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _viewModel.CloseRequested += Close;
        _viewModel.RequestEmulatorPath += OnRequestEmulatorPath;
        _viewModel.GetOwnerWindow += () => this;

        DataContext = _viewModel;
    }

    /// <summary>
    /// Initializes the window with the specified emulator path, launcher mode, and ROM paths.
    /// </summary>
    /// <param name="emulatorPath">Optional path to the MAME emulator executable.</param>
    /// <param name="isLauncherMode">If true, the window operates in launcher mode.</param>
    /// <param name="systemRomPath">Optional path to the system ROM.</param>
    /// <param name="listOfSecondaryRomPaths">Optional array of secondary ROM paths.</param>
    public void Initialize(string emulatorPath = null, bool isLauncherMode = true, string systemRomPath = null, string[] listOfSecondaryRomPaths = null)
    {
        _viewModel.Initialize(emulatorPath, isLauncherMode, systemRomPath, listOfSecondaryRomPaths);

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
            Filter = "MAME Executable|mame*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectMAMEEmulator") ?? "Select MAME Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
