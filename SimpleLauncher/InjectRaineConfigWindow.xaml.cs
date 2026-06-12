using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for injecting Raine emulator configuration settings.
/// </summary>
public partial class InjectRaineConfigWindow
{
    private readonly InjectRaineConfigViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectRaineConfigWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    public InjectRaineConfigWindow(InjectRaineConfigViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _viewModel.CloseRequested += Close;
        _viewModel.RequestEmulatorPath += OnRequestEmulatorPath;
        _viewModel.RequestFilePath += OnRequestFilePath;
        _viewModel.RequestFolderPath += OnRequestFolderPath;
        _viewModel.GetOwnerWindow += () => this;

        DataContext = _viewModel;
    }

    /// <summary>
    /// Initializes the window with the specified emulator path, launcher mode, and file paths.
    /// </summary>
    /// <param name="emulatorPath">Optional path to the Raine emulator executable.</param>
    /// <param name="isLauncherMode">If true, the window operates in launcher mode.</param>
    /// <param name="gameFilePath">Optional path to the game file.</param>
    /// <param name="systemRomPath">Optional path to the system ROM.</param>
    public void Initialize(string emulatorPath = null, bool isLauncherMode = true, string gameFilePath = null, string systemRomPath = null)
    {
        _viewModel.Initialize(emulatorPath, isLauncherMode, gameFilePath, systemRomPath);

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
            Filter = "Raine Executable|raine*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectExeTitle") ?? "Select Raine Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private static string OnRequestFilePath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "NeoGeo CD BIOS (neocd.bin)|neocd.bin|All Files (*.*)|*.*",
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectNeoCdBios") ?? "Select NeoGeo CD BIOS File"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private static string OnRequestFolderPath()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectRomDirectory") ?? "Select Raine ROM Directory"
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private void BtnSelectNeoCdBios_Click(object sender, RoutedEventArgs e)
    {
        var path = OnRequestFilePath();
        if (!string.IsNullOrEmpty(path))
        {
            _viewModel.RaineNeoCdBios = path;
        }
    }

    private void BtnSelectRaineRomDirectory_Click(object sender, RoutedEventArgs e)
    {
        var path = OnRequestFolderPath();
        if (!string.IsNullOrEmpty(path))
        {
            _viewModel.RaineRomDirectory = path;
        }
    }
}
