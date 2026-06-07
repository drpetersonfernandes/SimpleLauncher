#nullable enable

namespace SimpleLauncher.Core.Interfaces;

public interface IImageLoader
{
    Task<(Stream? image, bool isDefault)> LoadImageAsync(string? imagePath);
    byte[]? LoadImageBytes(string filePath);
}
