namespace SimpleLauncher.Interfaces;

public interface IFileLockService
{
    bool IsFileLocked(string filePath);
}
