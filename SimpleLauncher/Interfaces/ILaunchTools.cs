namespace SimpleLauncher.Interfaces;

public interface ILaunchTools
{
    Task BatchConvertIsoToXisoAsync();
    Task BatchConvertToChdAsync(string selectedRomFolder);
    Task BatchConvertToCompressedFileAsync();
    Task BatchConvertToRvzAsync();
    Task CreateBatchFilesForPs3GamesAsync();
    Task CreateBatchFilesForScummVmGamesAsync();
    Task CreateBatchFilesForWindowsGamesAsync();
    Task CreateBatchFilesForXbox360XblaGamesAsync();
    Task FindRomCoverLaunchAsync(string selectedImageFolder, string selectedRomFolder);
    Task GameCoverScraperAsync(string selectedImageFolder, string selectedRomFolder);
    Task RetroGameCoverDownloaderAsync(string selectedImageFolder, string selectedRomFolder);
    Task RomValidatorAsync();
}
