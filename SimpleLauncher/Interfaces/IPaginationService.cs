namespace SimpleLauncher.Interfaces;

public interface IPaginationService
{
    int CurrentPage { get; }
    int FilesPerPage { get; set; }
    int TotalFiles { get; }
    int PaginationThreshold { get; set; }

    void Initialize(IPaginationHost host);
    void Reset();
    List<string> ApplyPagination(List<string> allFiles);
    bool CanGoPrev();
    bool CanGoNext();
    void GoToPreviousPage();
    void GoToNextPage();
}
