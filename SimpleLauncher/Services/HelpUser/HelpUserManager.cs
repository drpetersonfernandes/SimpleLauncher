using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.HelpUser.Models;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.HelpUser;

public partial class HelpUserManager
{
    private const string FilePath = "parameters.md";

    // Regex to match Markdown H2 headers: ## System Name
    private static readonly Regex HeaderRegex = MyRegex();

    public List<SystemHelper> Systems { get; private set; } = [];

    public void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                // Notify developer
                const string contextMessage = "The file 'parameters.md' is missing.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.FileParametersMdIsMissingMessageBox();

                return;
            }

            string markdownContent;
            try
            {
                markdownContent = File.ReadAllText(FilePath);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Unable to load 'parameters.md'. The file may be corrupted or in use.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FailedToLoadParametersMdMessageBox();

                return;
            }

            if (string.IsNullOrWhiteSpace(markdownContent))
            {
                // Notify developer
                const string contextMessage = "The file 'parameters.md' is empty.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.FileParametersMdIsEmptyMessageBox();

                return;
            }

            var parsedSystems = ParseMarkdown(markdownContent);

            if (parsedSystems.Count == 0)
            {
                // Notify developer
                const string contextMessage = "No valid systems found in 'parameters.md' after processing.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.NoSystemInParametersMdMessageBox();

                return;
            }

            Systems = parsedSystems;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Unexpected error while loading 'parameters.md'.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorWhileLoadingParametersMdMessageBox();
        }
    }

    /// <summary>
    /// Parses the Markdown content and extracts system information.
    /// </summary>
    /// <param name="markdownContent">The raw Markdown content.</param>
    /// <returns>A list of SystemHelper objects parsed from the Markdown.</returns>
    private static List<SystemHelper> ParseMarkdown(string markdownContent)
    {
        var systems = new List<SystemHelper>();
        var matches = HeaderRegex.Matches(markdownContent);

        for (var i = 0; i < matches.Count; i++)
        {
            var currentMatch = matches[i];
            var systemName = currentMatch.Groups[1].Value.Trim();

            // Skip the title header (e.g., "# List of Parameters to use in the 'system.xml'")
            // or any H2 that appears to be a title/instruction rather than a system
            if (systemName.StartsWith("List of Parameters", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Calculate the content range for this system
            var contentStart = currentMatch.Index + currentMatch.Length;
            var contentEnd = i < matches.Count - 1
                ? matches[i + 1].Index
                : markdownContent.Length;
            var contentLength = contentEnd - contentStart;

            if (contentLength > 0)
            {
                var content = markdownContent.Substring(contentStart, contentLength).Trim();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    systems.Add(new SystemHelper
                    {
                        SystemName = systemName,
                        SystemHelperText = NormalizeText(content)
                    });
                }
            }
        }

        return systems;
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // First, normalize all line endings to \n
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Split by normalized line endings and process
        return string.Join(Environment.NewLine,
            text.Split('\n')
                .Select(static line => line.TrimStart()));
    }

    [GeneratedRegex(@"^##\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
