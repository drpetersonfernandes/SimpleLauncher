using System.Windows.Media.Imaging;

namespace SimpleLauncher.Services.LoadImages;

/// <summary>
///     Provides extension methods for converting streams and byte arrays to WPF BitmapImage objects.
/// </summary>
public static class BitmapImageConverter
{
    /// <summary>
    ///     Converts a stream to a frozen BitmapImage. The stream is disposed after conversion.
    /// </summary>
    /// <param name="stream">The stream containing image data, or null.</param>
    /// <returns>A frozen BitmapImage, or null if the stream is null or conversion fails.</returns>
    public static BitmapImage? ToBitmapImage(this Stream? stream)
    {
        if (stream == null) return null;

        try
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            // With CacheOption.OnLoad the image data is fully loaded during EndInit(),
            // so the source stream is no longer needed and can be released.
            stream.Dispose();

            return bitmapImage;
        }
        catch (Exception ex)
        {
            Log.Debug($"[BitmapImageConverter] ToBitmapImage failed: {ex.Message}");
            stream.Dispose();
            return null;
        }
    }

    /// <summary>
    ///     Converts a byte array to a frozen BitmapImage.
    /// </summary>
    /// <param name="imageBytes">The byte array containing image data, or null.</param>
    /// <returns>A frozen BitmapImage, or null if the array is null, empty, or conversion fails.</returns>
    public static BitmapImage? ToBitmapImage(this byte[]? imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0) return null;

        using var ms = new MemoryStream(imageBytes);
        return ms.ToBitmapImage();
    }
}