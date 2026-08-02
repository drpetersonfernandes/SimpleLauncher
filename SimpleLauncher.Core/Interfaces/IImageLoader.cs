namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to load images from file paths or return default images.
/// </summary>
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

    /// <summary>
    /// Reads image file bytes from disk, handling long paths and access errors.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>The image bytes, or null if the file could not be read.</returns>
    byte[]? LoadImageBytes(string filePath);
}
