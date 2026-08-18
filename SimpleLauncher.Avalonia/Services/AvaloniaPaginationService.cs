using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

/// <summary>
/// Manages file list pagination, handling page navigation, button states, and status labels.
/// Avalonia port of the WPF <c>PaginationService</c> (implements the same Core
/// <see cref="IPaginationService"/> contract; the label text comes from
/// <see cref="IResourceProvider"/> instead of WPF resource dictionaries).
/// </summary>
public class AvaloniaPaginationService : IPaginationService
{
    private readonly IResourceProvider _resourceProvider;
    private IPaginationHost _host = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaPaginationService"/> class.
    /// </summary>
    /// <param name="resourceProvider">The resource provider used to localize the status label.</param>
    public AvaloniaPaginationService(IResourceProvider resourceProvider)
    {
        _resourceProvider = resourceProvider;
    }

    /// <inheritdoc />
    public int CurrentPage { get; private set; } = 1;

    /// <inheritdoc />
    public int TotalFiles { get; private set; }

    /// <inheritdoc />
    public int FilesPerPage { get; set; }

    /// <inheritdoc />
    public int PaginationThreshold { get; set; }

    /// <inheritdoc />
    public void Initialize(IPaginationHost host)
    {
        _host = host;
    }

    /// <inheritdoc />
    public void Reset()
    {
        CurrentPage = 1;
        _host?.SetPrevPageButtonEnabled(false);
        _host?.SetNextPageButtonEnabled(false);
        _host?.ScrollToTop();
        _host?.UpdateTotalFilesLabel(null);
    }

    /// <inheritdoc />
    public bool CanGoPrev()
    {
        return CurrentPage > 1;
    }

    /// <inheritdoc />
    public bool CanGoNext()
    {
        var totalPages = (int)Math.Ceiling(TotalFiles / (double)FilesPerPage);
        return CurrentPage < totalPages;
    }

    /// <inheritdoc />
    public void GoToPreviousPage()
    {
        if (CanGoPrev())
        {
            CurrentPage--;
        }
    }

    /// <inheritdoc />
    public void GoToNextPage()
    {
        if (CanGoNext())
        {
            CurrentPage++;
        }
    }

    /// <inheritdoc />
    public IList<string> ApplyPagination(IList<string> allFiles)
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

    private string BuildStatusLabel(int startIndex, int endIndex, int total)
    {
        var template = _resourceProvider.GetString(
            "Pagination.Displaying",
            "Displaying files {0} to {1} out of {2} total");

        try
        {
            return string.Format(template, startIndex, endIndex, total);
        }
        catch (FormatException)
        {
            // Malformed translation (wrong placeholder count): fall back to English
            return $"Displaying files {startIndex} to {endIndex} out of {total} total";
        }
    }
}