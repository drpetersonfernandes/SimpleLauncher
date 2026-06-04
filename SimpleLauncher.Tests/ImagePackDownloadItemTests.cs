using SimpleLauncher.Services.DownloadService.Models;
using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class ImagePackDownloadItemTests
{
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var item = new ImagePackDownloadItem();

        Assert.Null(item.DisplayName);
        Assert.Null(item.DownloadUrl);
        Assert.Null(item.ExtractPath);
        Assert.Equal(DownloadButtonState.Idle, item.State);
    }

    [Fact]
    public void PropertiesCanBeSet()
    {
        var item = new ImagePackDownloadItem
        {
            DisplayName = "NES Pack",
            DownloadUrl = "https://example.com/pack.zip",
            ExtractPath = @"C:\images\nes"
        };

        Assert.Equal("NES Pack", item.DisplayName);
        Assert.Equal("https://example.com/pack.zip", item.DownloadUrl);
        Assert.Equal(@"C:\images\nes", item.ExtractPath);
    }

    [Fact]
    public void StateIdleConveniencePropertiesAreCorrect()
    {
        var item = new ImagePackDownloadItem { State = DownloadButtonState.Idle };

        Assert.True(item.IsIdle);
        Assert.False(item.IsDownloading);
        Assert.False(item.IsDownloaded);
        Assert.False(item.IsFailed);
        Assert.True(item.CanStartDownload);
    }

    [Fact]
    public void StateDownloadingConveniencePropertiesAreCorrect()
    {
        var item = new ImagePackDownloadItem { State = DownloadButtonState.Downloading };

        Assert.False(item.IsIdle);
        Assert.True(item.IsDownloading);
        Assert.False(item.IsDownloaded);
        Assert.False(item.IsFailed);
        Assert.False(item.CanStartDownload);
    }

    [Fact]
    public void StateDownloadedConveniencePropertiesAreCorrect()
    {
        var item = new ImagePackDownloadItem { State = DownloadButtonState.Downloaded };

        Assert.False(item.IsIdle);
        Assert.False(item.IsDownloading);
        Assert.True(item.IsDownloaded);
        Assert.False(item.IsFailed);
        Assert.False(item.CanStartDownload);
    }

    [Fact]
    public void StateFailedConveniencePropertiesAreCorrect()
    {
        var item = new ImagePackDownloadItem { State = DownloadButtonState.Failed };

        Assert.False(item.IsIdle);
        Assert.False(item.IsDownloading);
        Assert.False(item.IsDownloaded);
        Assert.True(item.IsFailed);
        Assert.True(item.CanStartDownload);
    }

    [Fact]
    public void StateChangeRaisesPropertyChanged()
    {
        var item = new ImagePackDownloadItem();
        var changedProperties = new List<string>();
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != null)
                changedProperties.Add(args.PropertyName);
        };

        item.State = DownloadButtonState.Downloading;

        Assert.Contains(nameof(ImagePackDownloadItem.State), changedProperties);
        Assert.Contains(nameof(ImagePackDownloadItem.IsIdle), changedProperties);
        Assert.Contains(nameof(ImagePackDownloadItem.IsDownloading), changedProperties);
        Assert.Contains(nameof(ImagePackDownloadItem.IsDownloaded), changedProperties);
        Assert.Contains(nameof(ImagePackDownloadItem.IsFailed), changedProperties);
        Assert.Contains(nameof(ImagePackDownloadItem.CanStartDownload), changedProperties);
    }

    [Fact]
    public void SameStateDoesNotRaisePropertyChanged()
    {
        var item = new ImagePackDownloadItem { State = DownloadButtonState.Idle };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.State = DownloadButtonState.Idle;

        Assert.False(eventRaised);
    }

    [Fact]
    public void CanStartDownloadIsTrueForIdleAndFailedOnly()
    {
        var item = new ImagePackDownloadItem
        {
            State = DownloadButtonState.Idle
        };

        Assert.True(item.CanStartDownload);

        item.State = DownloadButtonState.Failed;
        Assert.True(item.CanStartDownload);

        item.State = DownloadButtonState.Downloading;
        Assert.False(item.CanStartDownload);

        item.State = DownloadButtonState.Downloaded;
        Assert.False(item.CanStartDownload);
    }
}
