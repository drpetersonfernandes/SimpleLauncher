using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class WindowSelectionDialogWindow : Window
{
    private readonly WindowSelectionDialogViewModel _viewModel;

    public WindowSelectionDialogWindow() : this(App.ServiceProvider.GetRequiredService<WindowSelectionDialogViewModel>()) { }

    public WindowSelectionDialogWindow(WindowSelectionDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.DialogResultRequested += result =>
        {
            Close(result == true);
        };

        DataContext = _viewModel;
    }

    public void Initialize(IEnumerable<(IntPtr Handle, string Title)> windows)
    {
        _viewModel.Initialize(windows);
    }

    public IntPtr SelectedWindowHandle => _viewModel.SelectedWindowHandle;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
