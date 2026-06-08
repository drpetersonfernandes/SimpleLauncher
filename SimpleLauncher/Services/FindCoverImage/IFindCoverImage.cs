namespace SimpleLauncher.Services.FindCoverImage;

public interface IFindCoverImage
{
    string FindCoverImagePath(string fileNameWithoutExtension, string systemName, Services.SystemManager.SystemManager systemManager, Core.Services.SettingsManager.SettingsManager settings);
    double CalculateJaroWinklerSimilarity(string s1, string s2);
}