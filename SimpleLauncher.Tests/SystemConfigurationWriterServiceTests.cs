using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SystemConfiguration;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="SystemConfigurationWriterService"/> using real XML files in a temp directory.
/// The temp file is created before constructing the service so <see cref="DataFileLocation"/>
/// always resolves to the temp path (portable mode).
/// </summary>
public class SystemConfigurationWriterServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _systemXmlPath;
    private readonly ILogger _logger = new NoOpLogger();
    private SystemConfigurationWriterService _service = null!;

    public SystemConfigurationWriterServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"SL_SystemWriter_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
        _systemXmlPath = Path.Combine(_testDir, "system.xml");
        File.WriteAllText(_systemXmlPath, "<SystemConfigs />");
        File.SetLastWriteTimeUtc(_systemXmlPath, DateTime.UtcNow.AddHours(1));
    }

    private void CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["SystemXmlPath"] = _systemXmlPath
            })
            .Build();

        _service = new SystemConfigurationWriterService(configuration, _logger);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    private static Mock<ISystemManager> CreateSystemMock(
        string name,
        IEnumerable<string>? folders = null,
        string imageFolder = "images",
        IEnumerable<string>? formatsToSearch = null,
        IEnumerable<string>? formatsToLaunch = null,
        bool groupByFolder = false,
        bool disableRecursiveSearch = false,
        bool extractBeforeLaunch = false,
        IEnumerable<IEmulator>? emulators = null)
    {
        var mock = new Mock<ISystemManager>();
        mock.SetupGet(x => x.SystemName).Returns(name);
        mock.SetupGet(x => x.SystemFolders).Returns(folders?.ToList() ?? ["roms"]);
        mock.SetupGet(x => x.SystemImageFolder).Returns(imageFolder);
        mock.SetupGet(x => x.FileFormatsToSearch).Returns(formatsToSearch?.ToList() ?? ["zip"]);
        mock.SetupGet(x => x.FileFormatsToLaunch).Returns(formatsToLaunch?.ToList() ?? ["zip"]);
        mock.SetupGet(x => x.GroupByFolder).Returns(groupByFolder);
        mock.SetupGet(x => x.DisableRecursiveSearch).Returns(disableRecursiveSearch);
        mock.SetupGet(x => x.ExtractFileBeforeLaunch).Returns(extractBeforeLaunch);
        mock.SetupGet(x => x.Emulators).Returns(emulators?.ToList() ?? []);
        return mock;
    }

    private static Mock<IEmulator> CreateEmulatorMock(string name, string location = "emu.exe", string parameters = "-fullscreen")
    {
        var mock = new Mock<IEmulator>();
        mock.SetupGet(x => x.EmulatorName).Returns(name);
        mock.SetupGet(x => x.EmulatorLocation).Returns(location);
        mock.SetupGet(x => x.EmulatorParameters).Returns(parameters);
        mock.SetupGet(x => x.ReceiveANotificationOnEmulatorError).Returns(false);
        return mock;
    }

    private XDocument LoadXml() => XDocument.Load(_systemXmlPath);

    [Fact]
    public async Task SaveSystemAsync_CreatesWellFormedXmlWithSystemEntry()
    {
        CreateService();
        var system = CreateSystemMock("NES");

        await _service.SaveSystemAsync(system.Object);

        var doc = LoadXml();
        var systemNode = doc.Root!.Elements("SystemConfig").Single();
        Assert.Equal("NES", systemNode.Element("SystemName")!.Value, StringComparer.Ordinal);
        Assert.Equal("roms", systemNode.Element("SystemFolders")!.Element("SystemFolder")!.Value);
        Assert.Equal("images", systemNode.Element("SystemImageFolder")!.Value);
        Assert.Equal("zip", systemNode.Element("FileFormatsToSearch")!.Element("FormatToSearch")!.Value);
    }

    [Fact]
    public async Task SaveSystemAsync_WritesEmulatorsAndOptionalLinks()
    {
        CreateService();
        var emulator = CreateEmulatorMock("RetroArch", "C:\\emu\\retroarch.exe", "-L core");
        emulator.SetupGet(x => x.ImagePackDownloadLink).Returns("https://example.com/pack.zip");
        var system = CreateSystemMock("SNES", emulators: [emulator.Object]);

        await _service.SaveSystemAsync(system.Object);

        var doc = LoadXml();
        var emulatorNode = doc.Root!.Elements("SystemConfig").Single().Element("Emulators")!.Element("Emulator")!;
        Assert.Equal("RetroArch", emulatorNode.Element("EmulatorName")!.Value);
        Assert.Equal("C:\\emu\\retroarch.exe", emulatorNode.Element("EmulatorLocation")!.Value);
        Assert.Equal("https://example.com/pack.zip", emulatorNode.Element("ImagePackDownloadLink")!.Value);
        Assert.Null(emulatorNode.Element("ImagePackDownloadLink2"));
    }

    [Fact]
    public async Task SaveSystemAsync_SortsSystemsAlphabetically()
    {
        CreateService();

        await _service.SaveSystemAsync(CreateSystemMock("Zelda System").Object);
        await _service.SaveSystemAsync(CreateSystemMock("Atari").Object);
        await _service.SaveSystemAsync(CreateSystemMock("mame").Object);

        var names = LoadXml().Root!.Elements("SystemConfig")
            .Select(x => x.Element("SystemName")!.Value)
            .ToList();

        Assert.Equal(["Atari", "mame", "Zelda System"], names);
    }

    [Fact]
    public async Task SaveSystemAsync_SameNameTwice_UpdatesInsteadOfDuplicating()
    {
        CreateService();
        var first = CreateSystemMock("NES", folders: ["first-folder"]);
        var second = CreateSystemMock("NES", folders: ["second-folder"]);

        await _service.SaveSystemAsync(first.Object);
        await _service.SaveSystemAsync(second.Object);

        var doc = LoadXml();
        var systemNodes = doc.Root!.Elements("SystemConfig").ToList();
        Assert.Single(systemNodes);
        Assert.Equal("second-folder", systemNodes[0].Element("SystemFolders")!.Element("SystemFolder")!.Value);
    }

    [Fact]
    public async Task SaveSystemAsync_WithOriginalSystemName_RenamesExistingEntry()
    {
        CreateService();
        var original = CreateSystemMock("Old Name", folders: ["roms"]);
        await _service.SaveSystemAsync(original.Object);

        var renamed = CreateSystemMock("New Name", folders: ["roms"]);
        await _service.SaveSystemAsync(renamed.Object, originalSystemName: "Old Name");

        var doc = LoadXml();
        var systemNodes = doc.Root!.Elements("SystemConfig").ToList();
        Assert.Single(systemNodes);
        Assert.Equal("New Name", systemNodes[0].Element("SystemName")!.Value);
    }

    [Fact]
    public async Task SaveSystemAsync_EmptyFile_IsRecovered()
    {
        CreateService();
        await File.WriteAllTextAsync(_systemXmlPath, "   ");

        await _service.SaveSystemAsync(CreateSystemMock("NES").Object);

        Assert.Equal("NES", LoadXml().Root!.Elements("SystemConfig").Single().Element("SystemName")!.Value);
    }

    [Fact]
    public async Task SaveSystemAsync_CorruptXml_ThrowsInvalidOperationException()
    {
        CreateService();
        await File.WriteAllTextAsync(_systemXmlPath, "<SystemConfigs><broken>");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SaveSystemAsync(CreateSystemMock("NES").Object));
    }

    [Fact]
    public async Task DeleteSystemAsync_RemovesSystemNode()
    {
        CreateService();
        await _service.SaveSystemAsync(CreateSystemMock("NES").Object);
        await _service.SaveSystemAsync(CreateSystemMock("SNES").Object);

        await _service.DeleteSystemAsync("NES");

        var names = LoadXml().Root!.Elements("SystemConfig")
            .Select(x => x.Element("SystemName")!.Value)
            .ToList();
        Assert.Equal(["SNES"], names);
    }

    [Fact]
    public async Task DeleteSystemAsync_UnknownSystem_IsNoOp()
    {
        CreateService();
        await _service.SaveSystemAsync(CreateSystemMock("NES").Object);

        await _service.DeleteSystemAsync("Does Not Exist");

        Assert.Single(LoadXml().Root!.Elements("SystemConfig"));
    }

    [Fact]
    public async Task DeleteSystemAsync_MissingFile_IsNoOp()
    {
        CreateService();
        File.Delete(_systemXmlPath);

        await _service.DeleteSystemAsync("NES");
        // No exception expected
    }

    [Fact]
    public async Task SystemExists_ReturnsTrueForExistingSystem_CaseInsensitive()
    {
        CreateService();
        await _service.SaveSystemAsync(CreateSystemMock("NES").Object);

        Assert.True(_service.SystemExists("NES"));
        Assert.True(_service.SystemExists("nes"));
        Assert.False(_service.SystemExists("SNES"));
    }

    [Fact]
    public void SystemExists_MissingFile_ReturnsFalse()
    {
        CreateService();
        File.Delete(_systemXmlPath);

        Assert.False(_service.SystemExists("NES"));
    }
}
