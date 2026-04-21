namespace SimpleLauncher.Services.LaunchTools;

public interface ILaunchTools
{
    void BatchConvertIsoToXiso();
    void BatchConvertToChd(string selectedRomFolder);
    void BatchConvertToCompressedFile();
    void BatchConvertToRvz();
    void CreateBatchFilesForPs3Games();
    void CreateBatchFilesForScummVmGames();
    void CreateBatchFilesForWindowsGames();
    void CreateBatchFilesForXbox360XblaGames();
    void FindRomCoverLaunch(string selectedImageFolder, string selectedRomFolder);
    void GameCoverScraper(string selectedImageFolder, string selectedRomFolder);
    void RetroGameCoverDownloader(string selectedImageFolder, string selectedRomFolder);
    void RomValidator();
}