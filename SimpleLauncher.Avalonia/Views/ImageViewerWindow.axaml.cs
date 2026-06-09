using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class ImageViewerWindow : Window
{
    private readonly AvaloniaImageViewerViewModel _viewModel;

    public ImageViewerWindow(AvaloniaImageViewerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public void LoadImagePath(string imagePath)
    {
        _ = _viewModel.LoadImageFromPathAsync(imagePath);
    }

    public void LoadImageUrl(Uri imageUri)
    {
        _ = _viewModel.LoadImageFromUriAsync(imageUri);
    }
}
