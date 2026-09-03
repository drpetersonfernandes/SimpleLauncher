using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Services.HelpUser;

/// <summary>
///     Loads and parses the 'parameters.md' file into a list of system help entries.
/// </summary>
public partial class HelpUserManager
{
    // Resolve against the app base directory (not CWD): parameters.md ships next to
    // the executable, and depending on the working directory made the file appear
    // "missing" (modal reinstall dialog) whenever the app was launched from elsewhere.
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "parameters.md");

    // Regex to match Markdown H2 headers: ## System Name
    private static readonly Regex HeaderRegex = MyRegex();
    private readonly ILogger _logger;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HelpUserManager" /> class.
    /// </summary>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="messageBoxLibrary">The message box service for user notifications.</param>
    public HelpUserManager(ILogger logErrors, IMessageBoxLibraryService messageBoxLibrary)
    {
        _logger = logErrors;
        _messageBoxLibrary = messageBoxLibrary;
    }

    /// <summary>
    ///     Gets the list of systems parsed from the 'parameters.md' file.
    /// </summary>
    public IList<SystemHelper> Systems { get; private set; } = [];

    /// <summary>
    ///     Loads 'parameters.md', parses its contents, and populates the <see cref="Systems" /> list.
    ///     Notifies the user through message boxes when the file is missing, empty, or invalid.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                // Notify developer
                const string contextMessage = "The file 'parameters.md' is missing.";
                _logger.Warning(contextMessage);

                // Notify user
                await _messageBoxLibrary.FileParametersMdIsMissingMessageBoxAsync();

                return;
            }

            string markdownContent;
            try
            {
                markdownContent = await File.ReadAllTextAsync(FilePath);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Unable to load 'parameters.md'. The file may be corrupted or in use.";
                _logger.Error(ex, contextMessage);

                // Notify user
                await _messageBoxLibrary.FailedToLoadParametersMdMessageBoxAsync();

                return;
            }

            if (string.IsNullOrWhiteSpace(markdownContent))
            {
                // Notify developer
                const string contextMessage = "The file 'parameters.md' is empty.";
                _logger.Warning(contextMessage);

                // Notify user
                await _messageBoxLibrary.FileParametersMdIsEmptyMessageBoxAsync();

                return;
            }

            var parsedSystems = ParseMarkdown(markdownContent);

            if (parsedSystems.Count == 0)
            {
                // Notify developer
                const string contextMessage = "No valid systems found in 'parameters.md' after processing.";
                _logger.Warning(contextMessage);

                // Notify user
                await _messageBoxLibrary.NoSystemInParametersMdMessageBoxAsync();

                return;
            }

            Systems = parsedSystems;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Unexpected error while loading 'parameters.md'.";
            _logger.Error(ex, contextMessage);

            // Notify user
            await _messageBoxLibrary.ErrorWhileLoadingParametersMdMessageBoxAsync();
        }
    }

    /// <summary>
    ///     Parses the Markdown content and extracts system information.
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
            if (systemName.StartsWith("List of Parameters", StringComparison.OrdinalIgnoreCase)) continue;

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
        if (string.IsNullOrEmpty(text)) return "";

        // First, normalize all line endings to \n
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Split by normalized line endings and process
        return string.Join(Environment.NewLine,
            text.Split('\n')
                .Select(static line => line.TrimStart()));
    }

    [SuppressMessage("Meziantou.Analyzer", "MA0023:UseRegexOptionsExplicitCapture",
        Justification = "Capturing group is needed to extract the system name")]
    [GeneratedRegex(@"^##\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled, 1000)]
    private static partial Regex MyRegex();
}