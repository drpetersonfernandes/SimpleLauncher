using System.Windows;

namespace SimpleLauncher.Services.Pagination;

public class PaginationService : IPaginationService
{
    private IPaginationHost _host;

    public int CurrentPage { get; private set; } = 1;

    public int TotalFiles { get; private set; }

    public int FilesPerPage { get; set; }

    public int PaginationThreshold { get; set; }

    public void Initialize(IPaginationHost host)
    {
        _host = host;
    }

    public void Reset()
    {
        CurrentPage = 1;
        _host?.SetPrevPageButtonEnabled(false);
        _host?.SetNextPageButtonEnabled(false);
        _host?.ScrollToTop();
        _host?.UpdateTotalFilesLabel(null);
    }

    public bool CanGoPrev()
    {
        return CurrentPage > 1;
    }

    public bool CanGoNext()
    {
        var totalPages = (int)Math.Ceiling(TotalFiles / (double)FilesPerPage);
        return CurrentPage < totalPages;
    }

    public void GoToPreviousPage()
    {
        if (CanGoPrev())
        {
            CurrentPage--;
        }
    }

    public void GoToNextPage()
    {
        if (CanGoNext())
        {
            CurrentPage++;
        }
    }

    public List<string> ApplyPagination(List<string> allFiles)
    {
        TotalFiles = allFiles.Count;

        if (TotalFiles == 0)
        {
            _host?.AddNoFilesMessage();
            _host?.SetPrevPageButtonEnabled(false);
            _host?.SetNextPageButtonEnabled(false);
            _host?.UpdateTotalFilesLabel(BuildStatusLabel(0, 0, 0));
            return allFiles;
        }

        var startIndex = (CurrentPage - 1) * FilesPerPage + 1;
        var endIndex = Math.Min(startIndex + FilesPerPage - 1, TotalFiles);

        if (TotalFiles > PaginationThreshold)
        {
            allFiles = allFiles.Skip((CurrentPage - 1) * FilesPerPage).Take(FilesPerPage).ToList();
            UpdateButtonStates();
        }
        else
        {
            _host?.SetPrevPageButtonEnabled(false);
            _host?.SetNextPageButtonEnabled(false);
        }

        _host?.UpdateTotalFilesLabel(BuildStatusLabel(startIndex, endIndex, TotalFiles));
        return allFiles;
    }

    private void UpdateButtonStates()
    {
        _host?.SetPrevPageButtonEnabled(CurrentPage > 1);
        _host?.SetNextPageButtonEnabled(CurrentPage * FilesPerPage < TotalFiles);
    }

    private static string BuildStatusLabel(int startIndex, int endIndex, int total)
    {
        var displayingfiles0To = (string)Application.Current.TryFindResource("Displayingfiles0to") ?? "Displaying files 0 to";
        var outOf = (string)Application.Current.TryFindResource("outof") ?? "out of";
        var totalText = (string)Application.Current.TryFindResource("total") ?? "total";
        var displayingfiles = (string)Application.Current.TryFindResource("Displayingfiles") ?? "Displaying files";
        var to = (string)Application.Current.TryFindResource("to") ?? "to";

        return total == 0
            ? $"{displayingfiles0To} 0 {outOf} 0 {totalText}"
            : $"{displayingfiles} {(total > 0 ? startIndex : 0)} {to} {endIndex} {outOf} {total} {totalText}";
    }
}