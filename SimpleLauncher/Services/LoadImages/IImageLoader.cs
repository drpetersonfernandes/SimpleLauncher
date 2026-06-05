using System.Windows.Media.Imaging;

namespace SimpleLauncher.Services.LoadImages;

public interface IImageLoader
{
    Task<(BitmapSource image, bool isDefault)> LoadImageAsync(string imagePath);
    BitmapImage LoadBitmapImageSafe(string filePath);
}
