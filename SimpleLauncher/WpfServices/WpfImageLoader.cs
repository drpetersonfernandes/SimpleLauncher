#nullable enable

using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.WpfServices;

/// <summary>
/// WPF implementation of IImageLoader, loading images from the filesystem with fallback to a default image.
/// </summary>
public class WpfImageLoader(ILogErrors logErrors, IConfiguration configuration, IMessageBoxLibraryService messageBox) : IImageLoader
{
    private readonly ILogErrors _logErrors = logErrors;
    private readonly IMessageBoxLibraryService _messageBox = messageBox;

    private readonly string _defaultImagePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        configuration.GetValue<string>("DefaultImagePath") ?? Path.Combine("images", "default.png"));

    /// <summary>Asynchronously loads an image from the specified path, falling back to a default image on failure.</summary>
    public async Task<(Stream? image, bool isDefault)> LoadImageAsync(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return await LoadDefaultImageAsync();
        }

        try
        {
            var imageBytes = await Task.Run(() => LoadImageBytes(imagePath));

            if (imageBytes == null)
            {
                return await LoadDefaultImageAsync();
            }

            return (new MemoryStream(imageBytes), false);
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

    private async Task<(Stream? image, bool isDefault)> LoadDefaultImageAsync()
    {
        try
        {
            var imageBytes = await Task.Run(() => LoadImageBytes(_defaultImagePath));

            if (imageBytes == null)
            {
                const string contextMessage = "Failed to load global default image: images\\default.png.";
                _logErrors.LogAndForget(null, contextMessage);
                await _messageBox.DefaultImageNotFoundMessageBox();
                return (null, true);
            }

            return (new MemoryStream(imageBytes), true);
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to load global default image: images\\default.png.";
            _logErrors.LogAndForget(ex, contextMessage);
            await _messageBox.DefaultImageNotFoundMessageBox();
            return (null, true);
        }
    }

    /// <summary>Reads image file bytes from disk, handling long paths and access errors.</summary>
    public byte[]? LoadImageBytes(string filePath)
    {
        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath))
        {
            return null;
        }

        try
        {
            return File.ReadAllBytes(longPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logErrors.LogAndForget(ex, $"Failed to read image file '{filePath}'. It might be locked or permissions are insufficient.");
            return null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"An unexpected error occurred while reading image file '{filePath}'.");
            return null;
        }
    }
}