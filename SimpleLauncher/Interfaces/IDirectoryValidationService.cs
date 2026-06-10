namespace SimpleLauncher.Interfaces;

public interface IDirectoryValidationService
{
    bool IsWritableDirectory(string path);
}
