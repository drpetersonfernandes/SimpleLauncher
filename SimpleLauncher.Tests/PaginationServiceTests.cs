using SimpleLauncher.Services.Pagination;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests for <see cref="PaginationService"/> core pagination behavior including
/// page navigation, boundary checks, subset selection, and host initialization.
/// </summary>
public class PaginationServiceTests
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
    /// Verifies that the default current page is 1 when no pagination has been applied.
    /// </summary>
    [Fact]
    public void DefaultCurrentPageIs1()
    {
        var service = CreateService();
        Assert.Equal(1, service.CurrentPage);
    }

    /// <summary>
    /// Verifies that the default total files count is 0 when no pagination has been applied.
    /// </summary>
    [Fact]
    public void DefaultTotalFilesIs0()
    {
        var service = CreateService();
        Assert.Equal(0, service.TotalFiles);
    }

    /// <summary>
    /// Verifies that CanGoPrev returns false when on the first page.
    /// </summary>
    [Fact]
    public void CanGoPrevReturnsFalseOnFirstPage()
    {
        var service = CreateService();
        Assert.False(service.CanGoPrev());
    }

    /// <summary>
    /// Verifies that CanGoNext returns false when there are no files.
    /// </summary>
    [Fact]
    public void CanGoNextReturnsFalseWhenNoFiles()
    {
        var service = CreateService();
        Assert.False(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that CanGoNext returns true when more pages are available beyond the current page.
    /// </summary>
    [Fact]
    public void CanGoNextReturnsTrueWhenMorePages()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        Assert.True(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that CanGoNext returns false when on the last page.
    /// </summary>
    [Fact]
    public void CanGoNextReturnsFalseOnLastPage()
    {
        var service = CreateService(10, 5);
        var files = Enumerable.Range(1, 5).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        Assert.False(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that GoToNextPage increments the current page number.
    /// </summary>
    [Fact]
    public void GoToNextPageIncrementsPage()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        Assert.Equal(2, service.CurrentPage);
    }

    /// <summary>
    /// Verifies that GoToNextPage does not advance beyond the total number of pages.
    /// </summary>
    [Fact]
    public void GoToNextPageDoesNotExceedTotalPages()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        service.GoToNextPage();
        service.GoToNextPage(); // Should not go beyond page 2
        Assert.Equal(2, service.CurrentPage);
    }

    /// <summary>
    /// Verifies that GoToPreviousPage decrements the current page number.
    /// </summary>
    [Fact]
    public void GoToPreviousPageDecrementsPage()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        service.GoToPreviousPage();
        Assert.Equal(1, service.CurrentPage);
    }

    /// <summary>
    /// Verifies that GoToPreviousPage does not decrement below page 1.
    /// </summary>
    [Fact]
    public void GoToPreviousPageDoesNotGoBelow1()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToPreviousPage();
        service.GoToPreviousPage();
        Assert.Equal(1, service.CurrentPage);
    }

    /// <summary>
    /// Verifies that ApplyPagination returns a subset of files equal to FilesPerPage when
    /// the total file count exceeds the pagination threshold.
    /// </summary>
    [Fact]
    public void ApplyPaginationReturnsCorrectSubsetAboveThreshold()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// Verifies that ApplyPagination returns all files when the count is below the pagination threshold.
    /// </summary>
    [Fact]
    public void ApplyPaginationReturnsAllFilesBelowThreshold()
    {
        var service = CreateService(5, 100);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
    }

    /// <summary>
    /// Verifies that ApplyPagination returns an empty list when given an empty input.
    /// </summary>
    [Fact]
    public void ApplyPaginationReturnsEmptyListForEmptyInput()
    {
        var service = CreateService(5, 5);
        var result = service.ApplyPagination([]);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that ApplyPagination updates the TotalFiles property to reflect the input count.
    /// </summary>
    [Fact]
    public void ApplyPaginationUpdatesTotalFiles()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 15).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        Assert.Equal(15, service.TotalFiles);
    }

    /// <summary>
    /// Verifies that ApplyPagination on the second page returns the correct file subset.
    /// </summary>
    [Fact]
    public void ApplyPaginationSecondPageReturnsCorrectFiles()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        var result = service.ApplyPagination(files);
        Assert.Equal(5, result.Count);
        Assert.Equal("file6.zip", result[0]);
        Assert.Equal("file10.zip", result[4]);
    }

    /// <summary>
    /// Verifies that ApplyPagination on the last page returns only the remaining files.
    /// </summary>
    [Fact]
    public void ApplyPaginationLastPageReturnsRemainingFiles()
    {
        var service = CreateService(7, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        var result = service.ApplyPagination(files);
        Assert.Equal(3, result.Count);
    }

    /// <summary>
    /// Verifies that Reset sets the current page back to 1.
    /// </summary>
    [Fact]
    public void ResetSetsPageBackTo1()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        service.GoToNextPage();
        service.Reset();
        Assert.Equal(1, service.CurrentPage);
    }

    /// <summary>
    /// Verifies that calling Reset without initializing a host does not throw an exception.
    /// </summary>
    [Fact]
    public void ResetWithoutHostDoesNotThrow()
    {
        var service = CreateService();
        var exception = Record.Exception(service.Reset);
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that Initialize sets the host and that pagination interacts with it correctly.
    /// </summary>
    [Fact]
    public void InitializeSetsHost()
    {
        var service = CreateService();
        var host = new TestPaginationHost();
        service.Initialize(host);
        // Verify by applying pagination which calls host methods
        service.ApplyPagination([]);
        Assert.True(host.NoFilesMessageAdded);
    }

    /// <summary>
    /// Verifies that ApplyPagination handles an exact page boundary where file count equals FilesPerPage.
    /// </summary>
    [Fact]
    public void ApplyPaginationWithExactPageBoundary()
    {
        var service = CreateService(10, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    /// <summary>
    /// Verifies that ApplyPagination correctly handles a single file input.
    /// </summary>
    [Fact]
    public void ApplyPaginationSingleFile()
    {
        var service = CreateService(10, 5);
        var result = service.ApplyPagination(["single.zip"]);
        Assert.Single(result);
        Assert.Equal("single.zip", result[0]);
    }

    /// <summary>
    /// Verifies that the FilesPerPage property can be modified after construction.
    /// </summary>
    [Fact]
    public void FilesPerPageCanBeModified()
    {
        var service = CreateService(5);
        service.FilesPerPage = 20;
        Assert.Equal(20, service.FilesPerPage);
    }

    /// <summary>
    /// Verifies that the PaginationThreshold property can be modified after construction.
    /// </summary>
    [Fact]
    public void PaginationThresholdCanBeModified()
    {
        var service = CreateService(threshold: 10);
        service.PaginationThreshold = 50;
        Assert.Equal(50, service.PaginationThreshold);
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
}
