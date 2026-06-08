using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleLauncher.Avalonia;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "Simple Launcher";
    [ObservableProperty] private string _statusText = "Ready";
}
