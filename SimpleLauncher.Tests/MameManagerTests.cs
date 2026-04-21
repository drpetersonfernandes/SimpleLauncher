using MessagePack;
using SimpleLauncher.Services.MameManager;
using Xunit;

namespace SimpleLauncher.Tests;

public class MameManagerTests
{
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
}
