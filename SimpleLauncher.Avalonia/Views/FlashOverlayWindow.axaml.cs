using Avalonia;
using Avalonia.Controls;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class FlashOverlayWindow : Window
{
    private readonly FlashOverlayViewModel _viewModel;

    public FlashOverlayWindow(FlashOverlayViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.CloseRequested += Close;
        DataContext = _viewModel;
    }

    public async Task ShowFlashAsync()
    {
        Position = new PixelPoint(0, 0);

        Show();

        // Fade in
        FlashRectangle.Opacity = 0;
        for (double i = 0; i <= 1; i += 0.05)
        {
            FlashRectangle.Opacity = i;
            await Task.Delay(15);
        }

        // Fade out
        for (double i = 1; i >= 0; i -= 0.05)
        {
            FlashRectangle.Opacity = i;
            await Task.Delay(15);
        }

        _viewModel.OnAnimationCompleted();
    }
}
