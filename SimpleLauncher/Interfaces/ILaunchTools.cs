namespace SimpleLauncher.Interfaces;

public interface ILaunchTools
{
    void BatchConvertIsoToXiso();
    void BatchConvertToChd();
    void BatchConvertToCompressedFile();
    void BatchConvertToRvz();
    void CreateBatchFilesForPs3Games();
    void CreateBatchFilesForScummVmGames();
    void CreateBatchFilesForSegaModel3Games();
    void CreateBatchFilesForWindowsGames();
    void CreateBatchFilesForXbox360XblaGames();
    void FindRomCoverLaunch(string selectedImageFolder, string selectedRomFolder);
    void GameCoverScraper(string selectedImageFolder, string selectedRomFolder);
    void RetroGameCoverDownloader(string selectedImageFolder, string selectedRomFolder);
    void RomValidator();
}