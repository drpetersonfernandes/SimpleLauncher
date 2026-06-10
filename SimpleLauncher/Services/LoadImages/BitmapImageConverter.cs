#nullable enable

using System.Windows.Media.Imaging;

namespace SimpleLauncher.Services.LoadImages;

public static class BitmapImageConverter
{
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
            return bitmapImage;
        }
        catch
        {
            return null;
        }
    }

    public static BitmapImage? ToBitmapImage(this byte[]? imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0) return null;

        using var ms = new MemoryStream(imageBytes);
        return ms.ToBitmapImage();
    }
}
