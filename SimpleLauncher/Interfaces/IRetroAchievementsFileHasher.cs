namespace SimpleLauncher.Interfaces;

public interface IRetroAchievementsFileHasher
{
    Task<string> CalculateStandardMd5Async(string filePath);
    string CalculateFilenameHash(string filePath);
    Task<string> CalculateHeaderBasedMd5Async(string filePath, string systemName);
    Task<string> CalculateArduboyHashAsync(string filePath);
    Task<string> CalculateN64HashAsync(string filePath);
}
