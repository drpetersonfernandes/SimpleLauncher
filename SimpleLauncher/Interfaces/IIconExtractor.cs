namespace SimpleLauncher.Interfaces;

public interface IIconExtractor
{
    void SaveIconFromExe(string exePath, string savePath, ILogErrors logErrors);
}
