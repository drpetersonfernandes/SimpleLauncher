using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.InjectConfigWindows;

/// <summary>
/// Window for injecting Redream emulator configuration settings.
/// </summary>
public partial class InjectRedreamConfigWindow : Window
{
    private readonly InjectRedreamConfigViewModel _viewModel;
    private readonly IFilePickerService _filePicker;
    private readonly Func<Task<string?>> _requestEmulatorPathHandler;
    private readonly Func<Window> _getOwnerWindowHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectRedreamConfigWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    /// <param name="filePicker">The file picker service used to locate the emulator executable.</param>
    public InjectRedreamConfigWindow(InjectRedreamConfigViewModel viewModel, IFilePickerService filePicker)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _filePicker = filePicker;
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

    private async Task<string?> OnRequestEmulatorPath()
    {
        return await _filePicker.OpenFileAsync(
            "Select Redream Emulator",
            "Redream Executable|redream.exe|All Executables|*.exe");
    }
}
