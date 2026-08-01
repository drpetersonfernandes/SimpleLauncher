namespace SimpleLauncher.Interfaces;

public interface IGetListOfFilesService
{
    Task<IList<string>> GetFilesAsync(string directoryPath, IList<string> fileExtensions, bool disableRecursiveSearch, bool groupByFolder, CancellationToken cancellationToken = default);
}
