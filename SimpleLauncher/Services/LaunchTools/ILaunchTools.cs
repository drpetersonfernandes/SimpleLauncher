namespace SimpleLauncher.Services.LaunchTools;

public interface ILaunchTools
{
    Task BatchConvertIsoToXiso();
    Task BatchConvertToChd(string selectedRomFolder);
    Task BatchConvertToCompressedFile();
    Task BatchConvertToRvz();
    Task CreateBatchFilesForPs3Games();
    Task CreateBatchFilesForScummVmGames();
    Task CreateBatchFilesForWindowsGames();
    Task CreateBatchFilesForXbox360XblaGames();
    Task FindRomCoverLaunch(string selectedImageFolder, string selectedRomFolder);
    Task GameCoverScraper(string selectedImageFolder, string selectedRomFolder);
    Task RetroGameCoverDownloader(string selectedImageFolder, string selectedRomFolder);
    Task RomValidator();
}
