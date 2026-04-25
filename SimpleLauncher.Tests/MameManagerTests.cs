using MessagePack;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class MameManagerTests : IDisposable
{
    public MameManagerTests()
    {
        ServiceProviderMock.Install();
    }

    public void Dispose()
    {
        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void LoadFromDatValidDatFileReturnsDeserializedList()
    {
        var items = new List<MameManager>
        {
            new() { MachineName = "pacman", Description = "Pac-Man (Midway)" },
            new() { MachineName = "mspacman", Description = "Ms. Pac-Man" }
        };

        var bytes = MessagePackSerializer.Serialize(items);
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dat");
        File.WriteAllBytes(tempFile, bytes);

        try
        {
            var result = MameManager.LoadFromDat(tempFile);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, static m => m.MachineName == "pacman" && m.Description == "Pac-Man (Midway)");
            Assert.Contains(result, static m => m.MachineName == "mspacman" && m.Description == "Ms. Pac-Man");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadFromDatSingleItemReturnsCorrectItem()
    {
        var items = new List<MameManager>
        {
            new() { MachineName = "galaga", Description = "Galaga (Namco)" }
        };

        var bytes = MessagePackSerializer.Serialize(items);
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dat");
        File.WriteAllBytes(tempFile, bytes);

        try
        {
            var result = MameManager.LoadFromDat(tempFile);

            Assert.Single(result);
            Assert.Equal("galaga", result[0].MachineName);
            Assert.Equal("Galaga (Namco)", result[0].Description);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadFromDatCorruptedMessagePackReturnsEmptyList()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dat");
        File.WriteAllBytes(tempFile, [0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);

        try
        {
            var result = MameManager.LoadFromDat(tempFile);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadFromDatXmlContentReturnsEmptyList()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dat");
        File.WriteAllText(tempFile, "<xml><item>test</item></xml>");

        try
        {
            var result = MameManager.LoadFromDat(tempFile);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadFromDatEmptyFileReturnsEmptyList()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dat");
        File.WriteAllBytes(tempFile, []);

        try
        {
            var result = MameManager.LoadFromDat(tempFile);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
