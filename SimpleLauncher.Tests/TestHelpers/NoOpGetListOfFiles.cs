using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpGetListOfFiles : IGetListOfFilesService
{
    public Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, bool disableRecursiveSearch, bool groupByFolder, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<string>());
    }
}
