using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class ImageViewerWindow
{
    private readonly ImageViewerViewModel _viewModel;

    public ImageViewerWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        var logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
        _viewModel = new ImageViewerViewModel(logErrors);
        DataContext = _viewModel;
    }

    /// <summary>
    /// Loads an image from a file path.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    public void LoadImagePath(string imagePath)
    {
        _viewModel.LoadImageFromPath(imagePath);
    }

    /// <summary>
    /// Loads an image from a URI (local or web).
    /// </summary>
    /// <param name="imageUri">The URI of the image.</param>
    public void LoadImageUrl(Uri imageUri)
    {
        _viewModel.LoadImageFromUri(imageUri);
    }
}
