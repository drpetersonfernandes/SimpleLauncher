namespace SimpleLauncher.Core.Interfaces;

public interface IDirectoryValidationService
{
    bool IsWritableDirectory(string path);
}
