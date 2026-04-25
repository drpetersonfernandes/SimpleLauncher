using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Ensures that version metadata scattered across the repository stays in sync
/// with the canonical version defined in SimpleLauncher.csproj.
/// When a mismatch is detected the test automatically rewrites the file and
/// then fails so the developer can review the change before committing.
/// </summary>
public partial class VersionConsistencyTests
{
    private static string GetProjectFilePath(string relativePath)
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyLocation == null)
        {
            throw new InvalidOperationException("Could not determine executing assembly location.");
        }

        var path = Path.Combine(assemblyLocation, "..", "..", "..", "..", relativePath);
        return Path.GetFullPath(path);
    }

    private static string GetProjectVersion()
    {
        var csprojPath = GetProjectFilePath(Path.Combine("SimpleLauncher", "SimpleLauncher.csproj"));
        Assert.True(File.Exists(csprojPath), $"SimpleLauncher.csproj not found at {csprojPath}");

        var doc = XDocument.Load(csprojPath);
        var assemblyVersion = doc.Descendants("AssemblyVersion").FirstOrDefault()?.Value;
        Assert.False(string.IsNullOrWhiteSpace(assemblyVersion), "AssemblyVersion not found in SimpleLauncher.csproj");

        return assemblyVersion.Trim();
    }

    [Fact]
    public void AppManifestVersionMatchesProjectVersion()
    {
        var projectVersion = GetProjectVersion();
        var version = Version.Parse(projectVersion);

        // app.manifest requires a four-part version number
        var expectedVersion = new Version(
            version.Major,
            version.Minor,
            version.Build == -1 ? 0 : version.Build,
            version.Revision == -1 ? 0 : version.Revision).ToString();

        var manifestPath = GetProjectFilePath(Path.Combine("SimpleLauncher", "app.manifest"));
        Assert.True(File.Exists(manifestPath), $"app.manifest not found at {manifestPath}");

        var content = File.ReadAllText(manifestPath);
        var match = MyRegex().Match(content);
        Assert.True(match.Success, "Could not find assemblyIdentity version attribute in app.manifest");

        var currentVersion = match.Groups[1].Value;
        if (currentVersion == expectedVersion)
        {
            return;
        }

        var updatedContent = MyRegex1().Replace(content, $"""<assemblyIdentity version="{expectedVersion}""");

        File.WriteAllText(manifestPath, updatedContent);
        Assert.Fail($"app.manifest version was automatically updated from '{currentVersion}' to '{expectedVersion}'. " +
                    "Please review the change and commit it.");
    }

    [Fact]
    public void UpdaterVersionTxtMatchesProjectVersion()
    {
        var projectVersion = GetProjectVersion();
        var expectedContent = $"release{projectVersion}";

        var versionTxtPath = GetProjectFilePath(Path.Combine("Updater", "version.txt"));
        Assert.True(File.Exists(versionTxtPath), $"Updater/version.txt not found at {versionTxtPath}");

        var currentContent = File.ReadAllText(versionTxtPath).Trim();
        if (currentContent == expectedContent)
        {
            return;
        }

        File.WriteAllText(versionTxtPath, expectedContent + Environment.NewLine);
        Assert.Fail($"Updater/version.txt was automatically updated from '{currentContent}' to '{expectedContent}'. " +
                    "Please review the change and commit it.");
    }

    [GeneratedRegex("""
                    <assemblyIdentity\s+version="([^"]+)"
                    """)]
    private static partial Regex MyRegex();

    [GeneratedRegex("""
                    <assemblyIdentity\s+version="[^"]+"
                    """)]
    private static partial Regex MyRegex1();
}
