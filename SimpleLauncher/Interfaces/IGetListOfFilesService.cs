namespace SimpleLauncher.Interfaces;

public interface IGetListOfFilesService
{
    Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, bool disableRecursiveSearch, bool groupByFolder, CancellationToken cancellationToken = default);
}
