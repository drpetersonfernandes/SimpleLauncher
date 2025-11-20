namespace SimpleLauncher.Services;

public interface ILaunchTools
{
    void CreateBatchFilesForXbox360XblaGames();
    void CreateBatchFilesForWindowsGames();
    void FindRomCoverLaunch(string selectedImageFolder, string selectedRomFolder);
    void CreateBatchFilesForPs3Games();
    void BatchConvertIsoToXiso();
    void BatchConvertToChd();
    void BatchConvertToCompressedFile();
    void BatchConvertToRvz();
    void BatchVerifyChdFiles();
    void BatchVerifyCompressedFiles();
    void CreateBatchFilesForScummVmGames();
    void CreateBatchFilesForSegaModel3Games();
    void RomValidator();
    void GameCoverScraper(string selectedImageFolder, string selectedRomFolder);
}