using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class SoundConfigurationWindow : Window
{
    public SoundConfigurationWindow()
    {
        InitializeComponent();
    }

    public SoundConfigurationWindow(AvaloniaSoundConfigurationViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.SaveCompleted += Close;
        viewModel.CloseRequested += Close;
    }
}
