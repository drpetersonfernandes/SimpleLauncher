using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GameLauncher.MountFiles;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;
using Xunit.Sdk;

namespace SimpleLauncher.Tests;

/// <summary>
/// Integration tests that mount real CHD disc images with CHDMounter and verify the mount succeeds.
/// Tests are skipped at runtime when the CHD file, the CHDMounter tool, or the Dokan driver is unavailable.
/// </summary>
public sealed class MountChdFilesIntegrationTests
{
    private readonly ILogger _logger = new NoOpLogger();
    private readonly IMessageBoxLibraryService _messageBox = new NoOpMessageBoxLibraryService();

    public static TheoryData<string, string, int> ChdFiles => new()
    {
        // Microsoft Xbox (CHDMounter console index 16)
        { @"J:\Microsoft Xbox\007 - Everything or Nothing (USA).chd", "007 - Everything or Nothing (USA)", 16 },
        { @"J:\Microsoft Xbox\4x4 Evo 2 (USA).chd", "4x4 Evo 2 (USA)", 16 },
        { @"J:\Microsoft Xbox\007 - Agent Under Fire (USA).chd", "007 - Agent Under Fire (USA)", 16 },
        // Sony PlayStation 3 (CHDMounter console index 10)
        { @"X:\Sony PlayStation 3\007 - Blood Stone (USA) (En,Fr).chd", "007 - Blood Stone (USA) (En,Fr)", 10 },
        { @"X:\Sony PlayStation 3\007 - Quantum of Solace (USA) (En,Fr) (Collector's Edition).chd", "007 - Quantum of Solace (USA) (En,Fr) (Collector's Edition)", 10 },
        { @"X:\Sony PlayStation 3\3D Dot Game Heroes (USA).chd", "3D Dot Game Heroes (USA)", 10 },
        // SNK Neo Geo CD (CHDMounter console index 5)
        { @"J:\SNK Neo Geo CD\ADK World (Japan).chd", "ADK World (Japan)", 5 },
        { @"J:\SNK Neo Geo CD\Andro Dunos (France) (Unl).chd", "Andro Dunos (France) (Unl)", 5 },
        { @"J:\SNK Neo Geo CD\2020 Super Baseball (Japan) (En,Ja).chd", "2020 Super Baseball (Japan) (En,Ja)", 5 }
    };

    [Theory]
    [MemberData(nameof(ChdFiles))]
    public async Task MountRealChdSucceeds_And_UnmountsCleanly(string chdFilePath, string gameName, int consoleIndex)
    {
        if (!File.Exists(chdFilePath))
        {
            throw SkipException.ForSkip($"CHD file not found: {chdFilePath}");
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            throw SkipException.ForSkip("Dokan driver is not installed. CHD cannot be mounted.");
        }

        var chdMounterExePath = Path.Combine(AppContext.BaseDirectory, "tools", "CHDMounter", GetChdMounterExecutableName());
        if (!File.Exists(chdMounterExePath))
        {
            throw SkipException.ForSkip($"CHDMounter executable not found: {chdMounterExePath}");
        }

        var mountService = new MountChdFiles(_logger);

        string? driveRoot = null;

        await using (var mounted = await mountService.MountAsync(chdFilePath, consoleIndex, _logger, _messageBox))
        {
            Assert.True(mounted.IsMounted, $"Mount failed for '{gameName}' (console index {consoleIndex}).");

            Assert.False(string.IsNullOrEmpty(mounted.MountedPath), "Mounted path was empty.");
            Assert.False(string.IsNullOrEmpty(mounted.MountedDriveLetter), "Mounted drive letter was empty.");

            driveRoot = $"{mounted.MountedDriveLetter}:\\";
            Assert.True(Directory.Exists(driveRoot), $"Mounted drive {driveRoot} is not accessible.");

            var entries = Directory.GetFileSystemEntries(driveRoot);
            Assert.NotEmpty(entries);

            AssertSystemSpecificContent(consoleIndex, driveRoot, gameName);
        }

        // After DisposeAsync the CHDMounter process is killed and the drive should be gone.
        await WaitForDriveToDisappearAsync(driveRoot);
    }

    private static void AssertSystemSpecificContent(int consoleIndex, string driveRoot, string gameName)
    {
        if (consoleIndex == 16)
        {
            // Xbox XDVDFS discs always contain default.xbe at the root.
            Assert.True(
                File.Exists(Path.Combine(driveRoot, "default.xbe")),
                $"default.xbe not found on mounted Xbox CHD '{gameName}'.");
        }
        else if (consoleIndex == 10)
        {
            // PS3 discs always contain a PS3_GAME directory at the root.
            Assert.True(
                Directory.Exists(Path.Combine(driveRoot, "PS3_GAME")),
                $"PS3_GAME not found on mounted PS3 CHD '{gameName}'.");
        }
    }

    private static async Task WaitForDriveToDisappearAsync(string? driveRoot)
    {
        if (string.IsNullOrEmpty(driveRoot))
        {
            return;
        }

        const int maxRetries = 20;
        for (var i = 0; i < maxRetries; i++)
        {
            if (!Directory.Exists(driveRoot))
            {
                return;
            }

            await Task.Delay(500);
        }

        Assert.False(Directory.Exists(driveRoot), $"Drive {driveRoot} still exists after unmount.");
    }

    private static string GetChdMounterExecutableName()
    {
        return System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64
            ? "CHDMounter_arm64.exe"
            : "CHDMounter.exe";
    }
}
