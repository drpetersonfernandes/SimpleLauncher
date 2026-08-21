using SimpleLauncher.Avalonia.Interfaces;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Partial MainWindow implementing <see cref="ISystemSelectionHost"/> for system
/// selection coordination (WPF MainWindow.SystemSelectionHost.cs parity).
/// </summary>
public partial class MainWindow : ISystemSelectionHost
{
    void ISystemSelectionHost.SetSystemComboBoxItems(IReadOnlyList<string> systemNames)
    {
        SystemComboBox.ItemsSource = systemNames;
    }

    string? ISystemSelectionHost.GetSelectedSystem()
    {
        return SystemComboBox.SelectedItem as string;
    }

    void ISystemSelectionHost.SetEmulatorComboBoxItems(IReadOnlyList<string> emulatorNames)
    {
        EmulatorComboBox.ItemsSource = emulatorNames;
        EmulatorComboBox.SelectedIndex = emulatorNames.Count > 0 ? 0 : -1;
    }

    void ISystemSelectionHost.NavigateToSystem(string systemName)
    {
        _viewModel.NavigateToSystemCommand.Execute(systemName);
    }

    void ISystemSelectionHost.RefreshSidebar()
    {
        _systemManagerService.InvalidateCache();
        PopulateSidebarFromSystemXml();
    }

    void ISystemSelectionHost.RestartFileWatcher()
    {
        _fileWatcher.StartWatchingForSystems(_systemManagerService.LoadSystems());
    }
}
