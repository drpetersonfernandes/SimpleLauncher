using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class ImageViewerWindow
{
    private readonly ImageViewerViewModel _viewModel;

    public ImageViewerWindow(ImageViewerViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
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
