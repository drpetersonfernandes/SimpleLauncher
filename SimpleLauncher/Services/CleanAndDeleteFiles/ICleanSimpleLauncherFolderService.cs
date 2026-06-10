namespace SimpleLauncher.Services.CleanAndDeleteFiles;

public interface ICleanSimpleLauncherFolderService
{
    void CleanupTrash();
    void CleanupTempFiles();
}
