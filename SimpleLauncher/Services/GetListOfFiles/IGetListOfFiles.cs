#nullable enable
namespace SimpleLauncher.Services.GetListOfFiles;

public interface IGetListOfFiles
{
    Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, SystemManager.SystemManager systemManager, CancellationToken cancellationToken = default);
}
