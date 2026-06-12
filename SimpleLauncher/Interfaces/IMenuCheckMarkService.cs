namespace SimpleLauncher.Interfaces;

public interface IMenuCheckMarkService
{
    void Initialize(IMenuCheckMarkHost host);
    void UpdateThumbnailSizeCheckMarks(int selectedSize);
    void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize);
    void UpdateShowGamesCheckMarks(string selectedValue);
    void UpdateButtonAspectRatioCheckMarks(string selectedValue);
    void UpdateFilenameDisplayModeCheckMarks(string selectedValue);
    void UpdateFilenameFontSizeCheckMarks(string selectedValue);
    void UpdateMachineNameFontSizeCheckMarks(string selectedValue);
    void SetViewMode(string viewMode);
}
