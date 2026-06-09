using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class RetroAchievementsSettingsWindow : Window
{
    public RetroAchievementsSettingsWindow()
    {
        InitializeComponent();
    }

    public RetroAchievementsSettingsWindow(AvaloniaRetroAchievementsSettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
        viewModel.SaveCompleted += Close;
    }
}
