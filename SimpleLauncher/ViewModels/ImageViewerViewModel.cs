using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the ImageViewerWindow.
/// </summary>
public class ImageViewerViewModel : ObservableObject
{
    private BitmapSource _imageSource;
    private string _errorMessage;

    /// <summary>
    /// Gets or sets the image source to display.
    /// </summary>
    public BitmapSource ImageSource
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
    public void LoadImageFromPath(string imagePath)
    {
        try
        {
            var imageData = File.ReadAllBytes(imagePath);
            using var ms = new MemoryStream(imageData);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze(); // Freeze the bitmap to make it cross-thread accessible

            ImageSource = bitmap;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to load the image in the Image Viewer window.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ImageViewerErrorMessageBox();

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
                ErrorMessage = null;
            }
            else
            {
                ImageSource = null;
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load image from URI in ImageViewerWindow: {imageUri}");
            ImageSource = null;
        }
    }

    /// <summary>
    /// Clears the current image.
    /// </summary>
    public void ClearImage()
    {
        ImageSource = null;
        ErrorMessage = null;
    }
}
