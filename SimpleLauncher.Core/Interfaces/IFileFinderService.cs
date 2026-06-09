namespace SimpleLauncher.Core.Interfaces;

public interface IFileFinderService
{
    string FindDefaultXex(string directory);
    string FindDefaultXbe(string directory);
    string FindCueFile(string directory);
    string FindBinFile(string directory);
    string FindEbootBin(string directory);
    string FindImageIso(string directory);
}
