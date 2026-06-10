namespace SimpleLauncher.Interfaces;

public interface IFindCoverImageService
{
    string FindCoverImagePath(string fileNameWithoutExtension, string systemName, string systemImageFolder);
    double CalculateJaroWinklerSimilarity(string s1, string s2);
}
