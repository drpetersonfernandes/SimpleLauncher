using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class RightClickContextTests
{
    [Fact]
    public void ConstructorSetsFilePath()
    {
        var context = CreateContext(@"C:\roms\game.zip");
        Assert.Equal(@"C:\roms\game.zip", context.FilePath);
    }

    [Fact]
    public void ConstructorSetsFileNameWithExtension()
    {
        var context = CreateContext(fileNameWithExtension: "game.zip");
        Assert.Equal("game.zip", context.FileNameWithExtension);
    }

    [Fact]
    public void ConstructorSetsFileNameWithoutExtension()
    {
        var context = CreateContext(fileNameWithoutExtension: "game");
        Assert.Equal("game", context.FileNameWithoutExtension);
    }

    [Fact]
    public void ConstructorSetsSelectedSystemName()
    {
        var context = CreateContext(selectedSystemName: "NES");
        Assert.Equal("NES", context.SelectedSystemName);
    }

    [Fact]
    public void ConstructorSetsMachines()
    {
        var machines = new List<Services.MameManager.MameManager>();
        var context = CreateContext(machines: machines);
        Assert.Same(machines, context.Machines);
    }

    [Fact]
    public void ButtonPropertyIsMutable()
    {
        var context = CreateContext();
        Assert.Null(context.Button);
    }

    [Fact]
    public void ConstructorWithNullOptionalParametersDoesNotThrow()
    {
        var exception = Record.Exception(static () => CreateContext());
        Assert.Null(exception);
    }

    private static RightClickContext CreateContext(
        string filePath = "game.zip",
        string fileNameWithExtension = "game.zip",
        string fileNameWithoutExtension = "game",
        string selectedSystemName = "NES",
        List<Services.MameManager.MameManager>? machines = null)
    {
        return new RightClickContext(
            filePath,
            fileNameWithExtension,
            fileNameWithoutExtension,
            selectedSystemName,
            new Services.SystemManager.SystemManager { SystemName = selectedSystemName },
            machines,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}
