namespace SimpleLauncher.Core.Services.CleanAndDeleteFiles;

public interface ICleanSimpleLauncherFolderService
{
    void CleanupTrash();
    void CleanupTempFiles();
}
