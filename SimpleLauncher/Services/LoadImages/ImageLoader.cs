#nullable enable

using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.LoadImages;

public class ImageLoader(ILogErrors logErrors, IConfiguration configuration) : IImageLoader
{
    private readonly ILogErrors _logErrors = logErrors;

    private readonly string _defaultImagePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        configuration.GetValue<string>("DefaultImagePath") ?? Path.Combine("images", "default.png"));

    public async Task<(Stream? image, bool isDefault)> LoadImageAsync(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return await LoadDefaultImageAsync();
        }

        try
        {
            var imageBytes = await Task.Run(() => LoadImageBytes(imagePath));

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

            return (new MemoryStream(imageBytes), true);
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to load global default image: images\\default.png.";
            _logErrors.LogAndForget(ex, contextMessage);
            MessageBoxLibrary.DefaultImageNotFoundMessageBox();
            return (null, true);
        }
    }

    public byte[] LoadImageBytes(string filePath)
    {
        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath))
        {
            throw new FileNotFoundException($"Image file not found: {filePath}", filePath);
        }

        try
        {
            return File.ReadAllBytes(longPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Failed to read image file '{filePath}'. It might be locked or permissions are insufficient.", ex);
        }
        catch (Exception ex)
        {
            throw new IOException($"An unexpected error occurred while reading image file '{filePath}'.", ex);
        }
    }
}
