using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaImageViewerViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly HttpClient _httpClient;

    [ObservableProperty] private Bitmap? _imageSource;
    [ObservableProperty] private string? _errorMessage;

    public AvaloniaImageViewerViewModel(ILogErrors logErrors, IMessageBoxLibraryService messageBox, HttpClient httpClient)
    {
        _logErrors = logErrors;
        _messageBox = messageBox;
        _httpClient = httpClient;
    }

    public async Task LoadImageFromPathAsync(string imagePath)
    {
        try
        {
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            using var ms = new MemoryStream(imageBytes);
            ImageSource = new Bitmap(ms);
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to load the image in the Image Viewer window.";
            _logErrors.LogAndForget(ex, contextMessage);
            await _messageBox.ImageViewerErrorMessageBox();
            ImageSource = null;
        }
    }

    public async Task LoadImageFromUriAsync(Uri imageUri)
    {
        try
        {
            if (imageUri.Scheme is "http" or "https")
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUri);
                using var ms = new MemoryStream(imageBytes);
                ImageSource = new Bitmap(ms);
            }
            else if (imageUri.IsFile)
            {
                ImageSource = new Bitmap(imageUri.LocalPath);
            }
            else
            {
                ImageSource = new Bitmap(imageUri.ToString());
            }

            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to load image from URI in ImageViewerWindow: {imageUri}");
            ImageSource = null;
        }
    }

    public void ClearImage()
    {
        ImageSource = null;
        ErrorMessage = null;
    }
}
