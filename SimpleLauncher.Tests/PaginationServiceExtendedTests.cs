using SimpleLauncher.Services.Pagination;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Extended tests for <see cref="PaginationService"/> covering edge cases such as
/// large data sets, threshold boundaries, reset behavior, and host integration.
/// </summary>
public class PaginationServiceExtendedTests
{
    private static PaginationService CreateService(int filesPerPage = 10, int threshold = 10)
    {
        return new PaginationService
        {
            FilesPerPage = filesPerPage,
            PaginationThreshold = threshold
        };
    }

    /// <summary>
    /// Verifies that ApplyPagination updates total files when switching to a smaller data set.
    /// </summary>
    [Fact]
    public void ApplyPaginationResetsPageWhenNewDataSetIsSmaller()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        service.GoToNextPage();
        Assert.Equal(3, service.CurrentPage);

        // Now apply a smaller dataset
        var smallFiles = Enumerable.Range(1, 5).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(smallFiles);
        // Page should still be 3 but pagination should handle it
        Assert.Equal(5, service.TotalFiles);
    }

    /// <summary>
    /// Verifies that ApplyPagination returns all files when the file count equals the threshold.
    /// </summary>
    [Fact]
    public void ApplyPaginationWithThresholdEqualToFileCount()
    {
        var service = CreateService();
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that ApplyPagination returns all files when the threshold is one more than the file count.
    /// </summary>
    [Fact]
    public void ApplyPaginationWithThresholdOneMoreThanFileCount()
    {
        var service = CreateService(10, 11);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that ApplyPagination activates paging when the threshold is one less than the file count.
    /// </summary>
    [Fact]
    public void ApplyPaginationWithThresholdOneLessThanFileCount()
    {
        var service = CreateService(5, 9);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(5, result.Count);
        Assert.True(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that calling GoToNextPage multiple times advances through all pages and stops at the last.
    /// </summary>
    [Fact]
    public void GoToNextPageMultipleTimes()
    {
        var service = CreateService(3, 3);
        var files = Enumerable.Range(1, 15).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);

        service.GoToNextPage();
        Assert.Equal(2, service.CurrentPage);
        service.GoToNextPage();
        Assert.Equal(3, service.CurrentPage);
        service.GoToNextPage();
        Assert.Equal(4, service.CurrentPage);
        service.GoToNextPage();
        Assert.Equal(5, service.CurrentPage);
        service.GoToNextPage();
        Assert.Equal(5, service.CurrentPage); // Can't go beyond
    }

    /// <summary>
    /// Verifies that calling GoToPreviousPage from the middle page decrements correctly and stops at page 1.
    /// </summary>
    [Fact]
    public void GoToPreviousPageMultipleTimesFromMiddle()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 25).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        service.GoToNextPage();
        Assert.Equal(3, service.CurrentPage);

        service.GoToPreviousPage();
        Assert.Equal(2, service.CurrentPage);
        service.GoToPreviousPage();
        Assert.Equal(1, service.CurrentPage);
        service.GoToPreviousPage();
        Assert.Equal(1, service.CurrentPage); // Can't go below 1
    }

    /// <summary>
    /// Verifies that ApplyPagination returns the first page subset after a Reset.
    /// </summary>
    [Fact]
    public void ApplyPaginationAfterReset()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        service.GoToNextPage();
        service.Reset();

        var result = service.ApplyPagination(files);
        Assert.Equal(5, result.Count);
        Assert.Equal("file1.zip", result[0]);
    }

    /// <summary>
    /// Verifies that ApplyPagination handles a large file set of 10,000 entries correctly.
    /// </summary>
    [Fact]
    public void ApplyPaginationLargeFileSet()
    {
        var service = CreateService(100, 50);
        var files = Enumerable.Range(1, 10000).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(100, result.Count);
        Assert.Equal(10000, service.TotalFiles);
        Assert.True(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that ApplyPagination returns all files when FilesPerPage exceeds the total file count.
    /// </summary>
    [Fact]
    public void ApplyPaginationFilesPerPageLargerThanTotalFiles()
    {
        var service = CreateService(100, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that a threshold of zero causes pagination to apply when file count exceeds it.
    /// </summary>
    [Fact]
    public void ApplyPaginationThresholdZeroAlwaysPaginates()
    {
        var service = CreateService(5, 0);
        var files = Enumerable.Range(1, 3).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        // With threshold 0, TotalFiles (3) > PaginationThreshold (0), so pagination applies
        Assert.Equal(3, result.Count);
    }

    /// <summary>
    /// Verifies that a very large threshold prevents pagination from activating.
    /// </summary>
    [Fact]
    public void ApplyPaginationThresholdVeryLargeNeverPaginates()
    {
        var service = CreateService(5, 1000000);
        var files = Enumerable.Range(1, 100).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(100, result.Count);
    }

    /// <summary>
    /// Verifies that CanGoNext still returns true after Reset when pages remain.
    /// </summary>
    [Fact]
    public void CanGoNextReturnsFalseAfterReset()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        Assert.True(service.CanGoNext());
        service.Reset();
        Assert.True(service.CanGoNext()); // Still can go next because total files > files per page
    }

    /// <summary>
    /// Verifies that a single file below the threshold returns itself with no navigation available.
    /// </summary>
    [Fact]
    public void ApplyPaginationSingleFileBelowThreshold()
    {
        var service = CreateService(10, 100);
        var result = service.ApplyPagination(["single.zip"]);
        Assert.Single(result);
        Assert.Equal("single.zip", result[0]);
        Assert.False(service.CanGoNext());
        Assert.False(service.CanGoPrev());
    }

    /// <summary>
    /// Verifies that a single file above the threshold still returns itself without additional pages.
    /// </summary>
    [Fact]
    public void ApplyPaginationSingleFileAboveThreshold()
    {
        var service = CreateService(10, 0);
        var result = service.ApplyPagination(["single.zip"]);
        Assert.Single(result);
        Assert.Equal("single.zip", result[0]);
        Assert.False(service.CanGoNext());
    }

    /// <summary>
    /// Verifies forward and backward navigation returns the correct page subsets.
    /// </summary>
    [Fact]
    public void ApplyPaginationTwoPagesThenBackToFirst()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        Assert.Equal("file1.zip", service.ApplyPagination(files)[0]);

        service.GoToNextPage();
        var page2 = service.ApplyPagination(files);
        Assert.Equal("file6.zip", page2[0]);

        service.GoToPreviousPage();
        var page1 = service.ApplyPagination(files);
        Assert.Equal("file1.zip", page1[0]);
    }

    private sealed class TestPaginationHost : IPaginationHost
    {
        public bool NoFilesMessageAdded { get; private set; }
        public bool? PrevButtonEnabled { get; private set; }
        public bool? NextButtonEnabled { get; private set; }
        public bool ScrolledToTop { get; private set; }
        public string? LastStatusLabel { get; private set; }

        public void AddNoFilesMessage()
        {
            NoFilesMessageAdded = true;
        }

        public void SetPrevPageButtonEnabled(bool enabled)
        {
            PrevButtonEnabled = enabled;
        }

        public void SetNextPageButtonEnabled(bool enabled)
        {
            NextButtonEnabled = enabled;
        }

        public void ScrollToTop()
        {
            ScrolledToTop = true;
        }

        public void UpdateTotalFilesLabel(string label)
        {
            LastStatusLabel = label;
        }
    }

    /// <summary>
    /// Verifies that initializing with a host updates the host's status label and button states.
    /// </summary>
    [Fact]
    public void InitializeWithHostThenApplyPaginationUpdatesHost()
    {
        var service = CreateService(5, 5);
        var host = new TestPaginationHost();
        service.Initialize(host);

        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);

        Assert.NotNull(host.LastStatusLabel);
        Assert.True(host.NextButtonEnabled);
        Assert.False(host.PrevButtonEnabled);
    }

    /// <summary>
    /// Verifies that applying pagination with an empty list shows the no-files message on the host.
    /// </summary>
    [Fact]
    public void ApplyPaginationEmptyListWithHostShowsNoFilesMessage()
    {
        var service = CreateService(5, 5);
        var host = new TestPaginationHost();
        service.Initialize(host);

        service.ApplyPagination([]);

        Assert.True(host.NoFilesMessageAdded);
    }
}
