using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class SetGamepadDeadZoneWindow : Window
{
    public SetGamepadDeadZoneWindow()
    {
        InitializeComponent();
    }

    public SetGamepadDeadZoneWindow(AvaloniaSetGamepadDeadZoneViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.SaveCompleted += Close;
        viewModel.CloseRequested += Close;
    }
}
