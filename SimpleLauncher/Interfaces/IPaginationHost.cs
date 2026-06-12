namespace SimpleLauncher.Interfaces;

public interface IPaginationHost
{
    void SetPrevPageButtonEnabled(bool enabled);
    void SetNextPageButtonEnabled(bool enabled);
    void ScrollToTop();
    void UpdateTotalFilesLabel(string text);
    void AddNoFilesMessage();
}
