using SimpleLauncher.Services.AppDataFile;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="DataFileLocation"/> class.
/// </summary>
public class DataFileLocationTests
{
    [Fact]
    public void ConstructorSetsFileName()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        Assert.NotNull(location.FilePath);
        Assert.EndsWith(uniqueName, location.FilePath);
    }

    [Fact]
    public void TempFilePathAppendsTmpExtension()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        Assert.Equal(location.FilePath + ".tmp", location.TempFilePath);
    }

    [Fact]
    public void FilePathIsNotEmpty()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        Assert.NotEmpty(location.FilePath);
    }

    [Fact]
    public void IsPortableModeIsSet()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        // IsPortableMode should be a valid boolean (no exception)
        _ = location.IsPortableMode;
    }

    [Fact]
    public void GetLocalAppDataPathReturnsValidPath()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        var localPath = location.GetLocalAppDataPath();

        Assert.NotNull(localPath);
        Assert.EndsWith(uniqueName, localPath);
        Assert.Contains("SimpleLauncher", localPath);
    }

    [Fact]
    public void GetLocalAppDataPathContainsLocalAppData()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        var localPath = location.GetLocalAppDataPath();
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        Assert.StartsWith(localAppData, localPath);
    }

    [Fact]
    public void TryFallbackToLocalAppDataReturnsTrueAndUpdatesState()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        var result = location.TryFallbackToLocalAppData();

        Assert.True(result);
        Assert.False(location.IsPortableMode);
        Assert.Contains("SimpleLauncher", location.FilePath);
        Assert.EndsWith(uniqueName, location.FilePath);
    }

    [Fact]
    public void TryFallbackToLocalAppDataSetsCorrectPath()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location = new DataFileLocation(uniqueName);

        location.TryFallbackToLocalAppData();

        var expectedPath = location.GetLocalAppDataPath();
        Assert.Equal(expectedPath, location.FilePath);
    }

    [Fact]
    public void MultipleInstancesWithSameFileNameHaveSameLocalPath()
    {
        var uniqueName = $"testfile_{Guid.NewGuid():N}.xml";
        var location1 = new DataFileLocation(uniqueName);
        var location2 = new DataFileLocation(uniqueName);

        Assert.Equal(location1.GetLocalAppDataPath(), location2.GetLocalAppDataPath());
    }
}
