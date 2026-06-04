using SimpleLauncher.Services.GetListOfFiles;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpGetListOfFiles : IGetListOfFiles
{
    public Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, Services.SystemManager.SystemManager systemManager, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<string>());
    }
}
