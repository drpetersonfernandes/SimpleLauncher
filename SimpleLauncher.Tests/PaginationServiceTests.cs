using SimpleLauncher.Services.Pagination;
using Xunit;

namespace SimpleLauncher.Tests;

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

    [Fact]
    public void DefaultCurrentPageIs1()
    {
        var service = CreateService();
        Assert.Equal(1, service.CurrentPage);
    }

    [Fact]
    public void DefaultTotalFilesIs0()
    {
        var service = CreateService();
        Assert.Equal(0, service.TotalFiles);
    }

    [Fact]
    public void CanGoPrevReturnsFalseOnFirstPage()
    {
        var service = CreateService();
        Assert.False(service.CanGoPrev());
    }

    [Fact]
    public void CanGoNextReturnsFalseWhenNoFiles()
    {
        var service = CreateService();
        Assert.False(service.CanGoNext());
    }

    [Fact]
    public void CanGoNextReturnsTrueWhenMorePages()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        Assert.True(service.CanGoNext());
    }

    [Fact]
    public void CanGoNextReturnsFalseOnLastPage()
    {
        var service = CreateService(10, 5);
        var files = Enumerable.Range(1, 5).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        Assert.False(service.CanGoNext());
    }

    [Fact]
    public void GoToNextPageIncrementsPage()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        service.GoToNextPage();
        Assert.Equal(2, service.CurrentPage);
    }

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

    [Fact]
    public void ApplyPaginationReturnsCorrectSubsetAboveThreshold()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 20).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void ApplyPaginationReturnsAllFilesBelowThreshold()
    {
        var service = CreateService(5, 100);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public void ApplyPaginationReturnsEmptyListForEmptyInput()
    {
        var service = CreateService(5, 5);
        var result = service.ApplyPagination([]);
        Assert.Empty(result);
    }

    [Fact]
    public void ApplyPaginationUpdatesTotalFiles()
    {
        var service = CreateService(5, 5);
        var files = Enumerable.Range(1, 15).Select(static i => $"file{i}.zip").ToList();
        service.ApplyPagination(files);
        Assert.Equal(15, service.TotalFiles);
    }

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

    [Fact]
    public void ResetWithoutHostDoesNotThrow()
    {
        var service = CreateService();
        var exception = Record.Exception(service.Reset);
        Assert.Null(exception);
    }

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

    [Fact]
    public void ApplyPaginationWithExactPageBoundary()
    {
        var service = CreateService(10, 5);
        var files = Enumerable.Range(1, 10).Select(static i => $"file{i}.zip").ToList();
        var result = service.ApplyPagination(files);
        Assert.Equal(10, result.Count);
        Assert.False(service.CanGoNext());
    }

    [Fact]
    public void ApplyPaginationSingleFile()
    {
        var service = CreateService(10, 5);
        var result = service.ApplyPagination(["single.zip"]);
        Assert.Single(result);
        Assert.Equal("single.zip", result[0]);
    }

    [Fact]
    public void FilesPerPageCanBeModified()
    {
        var service = CreateService(5);
        service.FilesPerPage = 20;
        Assert.Equal(20, service.FilesPerPage);
    }

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
