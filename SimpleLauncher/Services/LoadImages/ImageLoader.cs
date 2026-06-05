using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.LoadImages;

public class ImageLoader(ILogErrors logErrors, IConfiguration configuration) : IImageLoader
{
    private readonly ILogErrors _logErrors = logErrors;

    private readonly string _defaultImagePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        configuration.GetValue<string>("DefaultImagePath") ?? Path.Combine("images", "default.png"));

    public async Task<(BitmapSource image, bool isDefault)> LoadImageAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return await LoadDefaultImageAsync();
        }

        try
        {
            var bitmapImage = await Task.Run(() => LoadBitmapImageSafe(imagePath));
            return (bitmapImage, false);
        }
        catch (NotSupportedException)
        {
            return await LoadDefaultImageAsync();
        }
        catch (Exception ex)
        {
            var contextMessage = $"Failed to load primary image: {imagePath}. Attempting to load default.";
            _logErrors.LogAndForget(ex, contextMessage);
            return await LoadDefaultImageAsync();
        }
    }

    private async Task<(BitmapSource image, bool isDefault)> LoadDefaultImageAsync()
    {
        try
        {
            var bitmapImage = await Task.Run(() => LoadBitmapImageSafe(_defaultImagePath));
            return (bitmapImage, true);
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to load global default image: images\\default.png.";
            _logErrors.LogAndForget(ex, contextMessage);
            MessageBoxLibrary.DefaultImageNotFoundMessageBox();
            return (null, true);
        }
    }

    public BitmapImage LoadBitmapImageSafe(string filePath)
    {
        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath))
        {
            throw new FileNotFoundException($"Image file not found: {filePath}", filePath);
        }

        byte[] imageData;
        try
        {
            imageData = File.ReadAllBytes(longPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Failed to read image file '{filePath}'. It might be locked or permissions are insufficient.", ex);
        }
        catch (Exception ex)
        {
            throw new IOException($"An unexpected error occurred while reading image file '{filePath}'.", ex);
        }

        try
        {
            using var ms = new MemoryStream(imageData);
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
        catch (NotSupportedException ex)
        {
            throw new NotSupportedException($"The image format or codec is not supported by the system: {filePath}", ex);
        }
    }
}
