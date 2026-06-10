using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.FindCoverImage;

namespace SimpleLauncher.Services.SystemImageResolver;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public class SystemImageResolverService : ISystemImageResolverService
{
    private readonly IConfiguration _configuration;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly Core.Services.SettingsManager.SettingsManager _settings;

    public SystemImageResolverService(IConfiguration configuration, IFindCoverImageService findCoverImage, Core.Services.SettingsManager.SettingsManager settings)
    {
        _configuration = configuration;
        _findCoverImage = findCoverImage;
        _settings = settings;
    }

    public Task<string> ResolveDisplayImageAsync(SystemManager.SystemManager config)
    {
        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = Path.Combine(appBaseDir, "images", "systems");
        var systemName = config.SystemName;
        var imageExtensions = _configuration.GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];

        foreach (var ext in imageExtensions)
        {
            var systemImagePath = Path.Combine(systemImageFolder, $"{systemName}{ext}");
            if (File.Exists(systemImagePath))
            {
                return Task.FromResult(systemImagePath);
            }
        }

        var enableFuzzyMatching = _settings.EnableFuzzyMatching;
        var similarityThreshold = _settings.FuzzyMatchingThreshold;

        if (enableFuzzyMatching && Directory.Exists(systemImageFolder))
        {
            var filesInImageFolder = Directory.GetFiles(systemImageFolder)
                .Where(f => imageExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            string bestMatchPath = null;
            double highestSimilarity = 0;
            var lowerSystemName = systemName.ToLowerInvariant();

            foreach (var filePath in filesInImageFolder)
            {
                var fileWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(fileWithoutExt)) continue;

                var lowerFileName = fileWithoutExt.ToLowerInvariant();
                var similarity = FindCoverImageService.CalculateJaroWinklerSimilarity(lowerSystemName, lowerFileName);

                if (!(similarity > highestSimilarity)) continue;

                highestSimilarity = similarity;
                bestMatchPath = filePath;
            }

            if (bestMatchPath != null && highestSimilarity >= similarityThreshold)
            {
                return Task.FromResult(bestMatchPath);
            }
        }

        var defaultImagePath = Path.Combine(systemImageFolder, "default.png");
        return Task.FromResult(File.Exists(defaultImagePath) ? defaultImagePath : Path.Combine(appBaseDir, "images", "default.png"));
    }
}
