namespace SimpleLauncher.Interfaces;

public interface IDeleteFilesService
{
    void TryDeleteFile(string filePath);
    Task TryDeleteFileAsync(string filePath);
}
