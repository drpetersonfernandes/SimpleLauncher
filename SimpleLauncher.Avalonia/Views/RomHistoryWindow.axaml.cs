using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class RomHistoryWindow : Window
{
    private readonly AvaloniaRomHistoryViewModel _viewModel;

    public RomHistoryWindow() : this(App.ServiceProvider.GetRequiredService<AvaloniaRomHistoryViewModel>()) { }

    public RomHistoryWindow(AvaloniaRomHistoryViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Opened += async (_, _) =>
        {
            try
            {
                await _viewModel.LoadRomHistoryAsync();
            }
            catch
            {
                // Error already logged in ViewModel
            }
        };
    }

    public void Initialize(string romName, string systemName, string searchTerm)
    {
        _viewModel.Initialize(romName, systemName, searchTerm);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
