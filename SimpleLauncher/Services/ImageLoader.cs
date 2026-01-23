using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services;

public static class ImageLoader
{
    private static readonly string GlobalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");

    /// <summary>
    /// Loads an image from the specified path asynchronously and safely.
    /// If the primary path fails, it attempts to load the global default image.
    /// </summary>
    /// <param name="imagePath">The primary path to the image file.</param>
    /// <returns>A tuple containing the loaded BitmapSource and a boolean indicating if the default image was loaded. Returns (null, true) if even the default fails.</returns>
    public static async Task<(BitmapSource image, bool isDefault)> LoadImageAsync(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            // If the provided path is invalid, immediately try loading the default.
            return await LoadDefaultImageAsync();
        }

        try
        {
            // Attempt to load the primary image asynchronously
            var bitmapImage = await Task.Run(() => LoadBitmapImageSafe(imagePath));

            // If successful, return the loaded image and false (not default)
            return (bitmapImage, false);
        }
        catch (Exception ex)
        {
            // Notify developer
            // If loading the primary image fails, log the error
            var contextMessage = $"Failed to load primary image: {imagePath}. Attempting to load default.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Then attempt to load the default image
            return await LoadDefaultImageAsync();
        }
    }

    /// <summary>
    /// Loads the global default image asynchronously and safely.
    /// </summary>
    /// <returns>A tuple containing the loaded default BitmapSource and true. Returns (null, true) if even the default fails.</returns>
    private static async Task<(BitmapSource image, bool isDefault)> LoadDefaultImageAsync()
    {
        try
        {
            // Attempt to load the default image asynchronously
            var bitmapImage = await Task.Run(static () => LoadBitmapImageSafe(GlobalDefaultImagePath));

            // If successful, return the loaded default image and true
            return (bitmapImage, true);
        }
        catch (Exception ex)
        {
            // Notify developer
            // If loading the default image fails, log a critical error
            const string contextMessage = "Failed to load global default image: images\\default.png.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            System.Windows.Application.Current.Dispatcher.Invoke(MessageBoxLibrary.DefaultImageNotFoundMessageBox);

            // Return null and true (indicating the default attempt failed)
            return (null, true);
        }
    }

    /// <summary>
    /// Safely loads a BitmapImage from a file path using a MemoryStream to prevent file locks.
    /// This method should be run on a background thread (e.g., via Task.Run).
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>The loaded BitmapImage.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    /// <exception cref="IOException">Thrown if there's an I/O error reading the file.</exception>
    /// <exception cref="System.Security.SecurityException">Thrown if there are permission issues.</exception>
    /// <exception cref="NotSupportedException">Thrown if the file format is not supported.</exception>
    private static BitmapImage LoadBitmapImageSafe(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Image file not found: {filePath}", filePath);
        }

        // Read the image into a memory stream to prevent file locks
        byte[] imageData;
        try
        {
            imageData = File.ReadAllBytes(filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
             // Wrap common file access errors for better context
             throw new IOException($"Failed to read image file '{filePath}'. It might be locked or permissions are insufficient.", ex);
        }
        catch (Exception ex)
        {
            // Catch any other exceptions during file reading
            throw new IOException($"An unexpected error occurred while reading image file '{filePath}'.", ex);
        }

        try
        {
            using var ms = new MemoryStream(imageData);
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad; // Ensures the image is loaded into memory
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze(); // Makes the image thread-safe

            return bi;
        }
        catch (NotSupportedException ex)
        {
            throw new NotSupportedException($"The image format or codec is not supported by the system: {filePath}", ex);
        }
    }
}