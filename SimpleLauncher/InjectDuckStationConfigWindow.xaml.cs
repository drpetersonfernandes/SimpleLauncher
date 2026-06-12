using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for injecting DuckStation emulator configuration settings.
/// </summary>
public partial class InjectDuckStationConfigWindow
{
    private readonly InjectDuckStationConfigViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectDuckStationConfigWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    public InjectDuckStationConfigWindow(InjectDuckStationConfigViewModel viewModel)
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
    /// Initializes the window with the specified emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">Optional path to the DuckStation emulator executable.</param>
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
            Filter = "DuckStation Executable|duckstation*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectDuckStationEmulator") ?? "Select DuckStation Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
