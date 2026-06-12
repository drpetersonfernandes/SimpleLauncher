using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for injecting PCSX2 emulator configuration settings.
/// </summary>
public partial class InjectPcsx2ConfigWindow
{
    private readonly InjectPcsx2ConfigViewModel _viewModel;

    public InjectPcsx2ConfigWindow(InjectPcsx2ConfigViewModel viewModel)
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
    /// <param name="emulatorPath">Optional path to the PCSX2 emulator executable.</param>
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
            Filter = "PCSX2 Executable|pcsx2*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectPCSX2Emulator") ?? "Select PCSX2 Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
