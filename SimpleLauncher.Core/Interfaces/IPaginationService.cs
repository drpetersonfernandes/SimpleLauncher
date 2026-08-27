namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Manages pagination state and navigation for the game file list.
/// </summary>
public interface IPaginationService
{
    /// <summary>
    /// Gets the current 1-based page number.
    /// </summary>
    int CurrentPage { get; }

    /// <summary>
    /// Gets or sets the number of files displayed per page.
    /// </summary>
    int FilesPerPage { get; set; }

    /// <summary>
    /// Gets the total number of files across all pages.
    /// </summary>
    int TotalFiles { get; }

    /// <summary>
    /// Gets or sets the file count threshold above which pagination is applied.
    /// </summary>
    int PaginationThreshold { get; set; }

    /// <summary>
    /// Initializes the service with a host that provides UI callbacks for pagination controls.
    /// </summary>
    /// <param name="host">The host providing pagination UI callbacks.</param>
    void Initialize(IPaginationHost host);

    /// <summary>
    /// Resets pagination to the first page and disables all navigation buttons.
    /// </summary>
    void Reset();

    /// <summary>
    /// Applies pagination to the file list, returning only the current page's files when the threshold is exceeded.
    /// </summary>
    /// <param name="allFiles">The complete list of files to paginate.</param>
    /// <returns>The files to display for the current page.</returns>
    IList<string> ApplyPagination(IList<string> allFiles);

    /// <summary>
    /// Determines whether navigation to the previous page is possible.
    /// </summary>
    /// <returns>True if a previous page exists; otherwise, false.</returns>
    bool CanGoPrev();

    /// <summary>
    /// Determines whether navigation to the next page is possible.
    /// </summary>
    /// <returns>True if a next page exists; otherwise, false.</returns>
    bool CanGoNext();

    /// <summary>
    /// Navigates to the previous page if available.
    /// </summary>
    void GoToPreviousPage();

    /// <summary>
    /// Navigates to the next page if available.
    /// </summary>
    void GoToNextPage();
}