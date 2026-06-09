namespace SimpleLauncher.Core.Interfaces;

public interface IFileLockService
{
    bool IsFileLocked(string filePath);
}
