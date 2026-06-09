namespace SimpleLauncher.Core.Interfaces;

public interface IFindCoverImageService
{
    string FindCoverImagePath(string fileNameWithoutExtension, string systemName, string systemImageFolder);
}
