using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the ImageViewerWindow.
/// </summary>
public class ImageViewerViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IMessageBoxLibraryService _messageBox;
    private BitmapSource? _imageSource;
    private string _errorMessage = "";

    /// <summary>Initializes a new instance of the <see cref="ImageViewerViewModel"/>.</summary>
    /// <param name="logErrors">The logger instance.</param>
    /// <param name="messageBox">The message box service for error notifications.</param>
    public ImageViewerViewModel(ILogger logErrors, IMessageBoxLibraryService messageBox)
    {
        _logger = logErrors;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Gets or sets the image source to display.
    /// </summary>
    public BitmapSource? ImageSource
    {
        get => _imageSource;
        private set => SetProperty(ref _imageSource, value);
    }

    /// <summary>
    /// Gets or sets an error message if image loading failed.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Loads an image from a file path.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    public async Task LoadImageFromPathAsync(string? imagePath)
    {
        try
        {
            var imageData = await File.ReadAllBytesAsync(imagePath!);
            using var ms = new MemoryStream(imageData);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze(); // Freeze the bitmap to make it cross-thread accessible

            ImageSource = bitmap;
            ErrorMessage = "";
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to load the image in the Image Viewer window.";
            _logger.Error(ex, contextMessage);

            // Notify user
            await _messageBox.ImageViewerErrorMessageBoxAsync();

            ImageSource = null;
        }
    }

    /// <summary>
    /// Loads an image from a URI (local or web).
    /// </summary>
    /// <param name="imageUri">The URI of the image.</param>
    public void LoadImageFromUri(Uri imageUri)
    {
        try
        {
            if (imageUri != null)
            {
                var bitmap = new BitmapImage(imageUri);
                bitmap.Freeze();
                ImageSource = bitmap;
                ErrorMessage = "";
            }
            else
            {
                ImageSource = null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to load image from URI in ImageViewerWindow: {imageUri}");
            ImageSource = null;
        }
    }
}