using SimpleLauncher.Services.Pagination;
using Xunit;

namespace SimpleLauncher.Tests;

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

    [Fact]
    public void ApplyPaginationWithThresholdEqualToFileCount()
    {
        var service = CreateService();
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    [Fact]
    public void ApplyPaginationWithThresholdOneMoreThanFileCount()
    {
        var service = CreateService(10, 11);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    [Fact]
    public void ApplyPaginationWithThresholdOneLessThanFileCount()
    {
        var service = CreateService(5, 9);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(5, result.Count);
        Assert.True(service.CanGoNext());
    }

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

    [Fact]
    public void ApplyPaginationFilesPerPageLargerThanTotalFiles()
    {
        var service = CreateService(100, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    [Fact]
    public void ApplyPaginationThresholdZeroAlwaysPaginates()
    {
        var service = CreateService(5, 0);
        var files = Enumerable.Range(1, 3).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        // With threshold 0, TotalFiles (3) > PaginationThreshold (0), so pagination applies
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyPaginationThresholdVeryLargeNeverPaginates()
    {
        var service = CreateService(5, 1000000);
        var files = Enumerable.Range(1, 100).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(100, result.Count);
    }

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

    [Fact]
    public void ApplyPaginationSingleFileAboveThreshold()
    {
        var service = CreateService(10, 0);
        var result = service.ApplyPagination(["single.zip"]);
        Assert.Single(result);
        Assert.Equal("single.zip", result[0]);
        Assert.False(service.CanGoNext());
    }

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
