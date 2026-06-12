using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for displaying images from local paths or URIs.
/// </summary>
public partial class ImageViewerWindow
{
    private readonly ImageViewerViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewerWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing image viewing logic.</param>
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
        _ = _viewModel.LoadImageFromPathAsync(imagePath);
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
