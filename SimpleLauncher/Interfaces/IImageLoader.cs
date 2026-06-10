#nullable enable
namespace SimpleLauncher.Interfaces;

public interface IImageLoader
{
    Task<(Stream? image, bool isDefault)> LoadImageAsync(string? imagePath);
    byte[]? LoadImageBytes(string filePath);
}
