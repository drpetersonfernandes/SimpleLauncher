#nullable enable

namespace SimpleLauncher.Interfaces;

public interface IImageLoader
{
    /// <summary>
    /// Loads an image from the specified path.
    /// </summary>
    /// <param name="imagePath">The path to the image file, or null to load the default image.</param>
    /// <returns>
    /// A tuple containing the image stream and a flag indicating whether the default image was used.
    /// The caller takes ownership of the returned stream and is responsible for disposing it.
    /// </returns>
    Task<(Stream? image, bool isDefault)> LoadImageAsync(string? imagePath);

    byte[]? LoadImageBytes(string filePath);
}
