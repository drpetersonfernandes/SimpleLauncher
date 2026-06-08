namespace SimpleLauncher.Core.Services.CleanAndDeleteFiles;

public interface IDeleteFilesService
{
    void TryDeleteFile(string filePath);
    Task TryDeleteFileAsync(string filePath);
}
