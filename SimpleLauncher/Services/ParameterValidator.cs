using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleLauncher.Services;

/// <summary>
/// Provides methods for validating file paths and parameters used in emulator configurations.
/// </summary>
public static partial class ParameterValidator
{
    // Regex to split the command line into tokens, respecting quotes.
    [GeneratedRegex("""[\"](.+?)[\"]|([^ ]+)""", RegexOptions.Compiled)]
    private static partial Regex CommandLineTokenizerRegex();

    // Known parameter placeholders that shouldn't be validated as actual paths
    private static readonly string[] KnownPlaceholders =
    [
        "%ROM%", "%GAME%", "%ROMNAME%", "%ROMFILE%", "$rom$", "$game$", "$romname$", "$romfile$",
        "{rom}", "{game}", "{romname}", "{romfile}"
    ];

    // Known parameter flags that shouldn't be validated as actual paths
    // This list is extensive to avoid misinterpreting flags as paths.
    private static readonly string[] KnownParameterFlags =
    [
        "-f", "--fullscreen", "/f", "-window", "-fullscreen", "--window", "-cart",
        "-L", "-g", "-rompath", "-cdrom", "-harddisk", "-flop1", "-flop2", "-cass", "-skip_bios",
        "-bios", "-cart1", "-cart2", "-exp", "-megacd", "-32x", "-addon", "-force_system",
        "-system", "-loadstate", "-savestate", "-record", "-playback", "-cheats", "-debug",
        "-nogui", "-batch", "-exit", "-config", "-input", "-output", "-sound", "-video",
        "-joy", "-keyboard", "-mouse", "-lightgun", "-joystick", "-paddle", "-dial", "-trackball",
        "-pedal", "-adstick", "-port", "-device", "-listxml", "-listfull", "-listsnap",
        "-listsoftware", "-verifyroms", "-verifysamples", "-verifysoftlist", "-createconfig",
        "-showconfig", "-showusage", "-validate", "-autoframeskip", "-frameskip", "-throttle",
        "-nothrottle", "-sleep", "-nosleep", "-speed", "-refresh", "-resolution", "-aspect",
        "-view", "-screen", "-artwork_crop", "-use_artwork_crop", "-allow_artwork_crop",
        "-keepaspect", "-unevenstretch", "-unevenstretch_x", "-unevenstretch_y", "-effect",
        "-waitvsync", "-nowaitvsync", "-syncrefresh", "-nosyncrefresh", "-triplebuffer",
        "-notriplebuffer", "-switchres", "-noswitchres", "-maximize", "-nomaximize",
        "-snapsize", "-snapview", "-snapbilinear", "-snapdither",
        "-snapdither_pattern", "-snapdither_threshold", "-snapalias", "-burnin", "-noburnin",
        "-autoboot_command", "-autoboot_delay", "-autoboot_script", "-exit_time", "-into_menu",
        "-menu_theme", "-menu_font_size", "-menu_font_game_size", "-menu_wrap_text",
        "-listmedia", "-listdevices", "-listmidi", "-listnetwork", "-listinputs", "-listcontrollers",
        "-listlights", "-listspeakers", "-listmonitors", "-listvideo", "-listpalette",
        "-listcolors", "-listkeys", "-listcommands"
    ];

    private static readonly char[] PathSeparators = ['\\', '/'];
    private static readonly char[] ArgumentSeparators = [';'];

    private static bool LooksLikePath(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (text.Length < 3 && text.All(char.IsDigit)) return false;

        return text.IndexOfAny(PathSeparators) >= 0 ||
               (text.Length >= 2 && text[1] == ':' && char.IsLetter(text[0])) ||
               text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".rom", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".cue", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".iso", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".chd", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDirectoryPath(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        return text.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
               text.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
               (!string.IsNullOrEmpty(text) && string.IsNullOrEmpty(Path.GetFileName(text)) && Directory.Exists(PathHelper.ResolveRelativeToAppDirectory(text)));
    }

    private static bool IsKnownFlag(string text)
    {
        return KnownParameterFlags.Any(flag =>
            string.Equals(text, flag, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsPlaceholder(string path)
    {
        return KnownPlaceholders.Any(placeholder =>
            path.Contains(placeholder, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryResolveAndValidateSinglePath(string path, string systemFolder, out string resolvedPath)
    {
        resolvedPath = path;
        if (string.IsNullOrWhiteSpace(path) || ContainsPlaceholder(path)) return true;

        var pathForChecking = path.Contains('%') ? Environment.ExpandEnvironmentVariables(path) : path;

        try
        {
            if (Path.IsPathRooted(pathForChecking))
            {
                resolvedPath = Path.GetFullPath(pathForChecking);
                return File.Exists(resolvedPath) || Directory.Exists(resolvedPath);
            }

            if (!string.IsNullOrEmpty(systemFolder))
            {
                var systemRelative = PathHelper.CombineAndResolveRelativeToAppDirectory(systemFolder, pathForChecking);
                if (File.Exists(systemRelative) || Directory.Exists(systemRelative))
                {
                    resolvedPath = systemRelative;
                    return true;
                }
            }

            var appRelative = PathHelper.ResolveRelativeToAppDirectory(pathForChecking);
            if (File.Exists(appRelative) || Directory.Exists(appRelative))
            {
                resolvedPath = appRelative;
                return true;
            }

            resolvedPath = appRelative;
            return false;
        }
        catch (Exception)
        {
            try
            {
                resolvedPath = PathHelper.ResolveRelativeToAppDirectory(pathForChecking);
            }
            catch
            {
                /* ignore */
            }

            return false;
        }
    }

    public static (bool overallValid, List<string> allInvalidPaths) ValidateParameterPaths(string parameters, string systemFolder = null, bool isMameSystem = false)
    {
        var invalidPaths = new List<string>();
        if (string.IsNullOrWhiteSpace(parameters)) return (true, invalidPaths);

        var allPathsCurrentlyValid = true;
        var tokens = CommandLineTokenizerRegex().Matches(parameters).Select(m => m.Value).ToList();

        for (var i = 0; i < tokens.Count; i++)
        {
            var tokenValue = tokens[i].Trim('"', '\'');
            if (ContainsPlaceholder(tokenValue) || IsKnownFlag(tokenValue)) continue;

            var isPathArgumentForFlag = i > 0 && IsKnownFlag(tokens[i - 1]) &&
                                        tokens[i - 1].ToLowerInvariant() is "-l" or "-rompath" or "-cart" or "-flop1" or "-bios" or "-config";

            if (!isPathArgumentForFlag && !LooksLikePath(tokenValue)) continue;

            if (tokenValue.Contains(';'))
            {
                foreach (var subPath in tokenValue.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmedSubPath = subPath.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedSubPath) || ContainsPlaceholder(trimmedSubPath) ||
                        TryResolveAndValidateSinglePath(trimmedSubPath, systemFolder, out _)) continue;

                    invalidPaths.Add(trimmedSubPath);
                    allPathsCurrentlyValid = false;
                }
            }
            else if (!TryResolveAndValidateSinglePath(tokenValue, systemFolder, out _))
            {
                invalidPaths.Add(tokenValue);
                allPathsCurrentlyValid = false;
            }
        }

        if (!isMameSystem || allPathsCurrentlyValid || invalidPaths.Count == 0)
            return (allPathsCurrentlyValid, invalidPaths);

        var hasCriticalInvalidPath = invalidPaths.Any(invalidPath =>
            Enumerable.Range(0, tokens.Count - 1)
                .Any(idx => tokens[idx + 1].Trim('"', '\'').Contains(invalidPath) &&
                            (tokens[idx].Equals("-L", StringComparison.OrdinalIgnoreCase) || tokens[idx].Equals("-rompath", StringComparison.OrdinalIgnoreCase))));
        if (!hasCriticalInvalidPath) return (true, invalidPaths);

        return (allPathsCurrentlyValid, invalidPaths);
    }

    public static (bool success, List<string> invalidPaths) ValidateEmulatorParameters(string parameters, string systemFolder = null, bool isMameSystem = false)
    {
        return ValidateParameterPaths(parameters, systemFolder, isMameSystem);
    }

    public static (string resolvedParameters, List<string> unresolvedOrInvalidPaths) ResolveAndConvertParameterPaths(string parameters, string systemFolder = null)
    {
        if (string.IsNullOrWhiteSpace(parameters))
        {
            return (parameters, new List<string>());
        }

        var unresolvedPathsList = new List<string>();
        var finalTokens = new List<string>();
        var originalTokens = CommandLineTokenizerRegex().Matches(parameters)
            .Select(static m => m.Value)
            .ToList();

        var currentIndex = 0;
        while (currentIndex < originalTokens.Count)
        {
            var currentToken = originalTokens[currentIndex];
            var unquotedCurrentToken = currentToken.Trim('"', '\'');

            if (ContainsPlaceholder(unquotedCurrentToken))
            {
                finalTokens.Add(currentToken);
                currentIndex++;
                continue;
            }

            if (IsKnownFlag(currentToken))
            {
                finalTokens.Add(currentToken);
                var lowerFlag = currentToken.ToLowerInvariant();
                currentIndex++; // Move past the flag

                if (lowerFlag is "-l" or "-rompath" or "-cart" or "-flop1" or "-bios" or "-config"
                    && currentIndex < originalTokens.Count)
                {
                    var pathCandidateBuilder = new StringBuilder();
                    var firstPartOfPath = originalTokens[currentIndex].Trim('"', '\'');
                    pathCandidateBuilder.Append(firstPartOfPath);
                    var pathTokensConsumed = 1;

                    // Greedily consume subsequent tokens if they form a valid path together
                    var lookAheadIndex = currentIndex + 1;
                    while (lookAheadIndex < originalTokens.Count &&
                           !IsKnownFlag(originalTokens[lookAheadIndex]) &&
                           !ContainsPlaceholder(originalTokens[lookAheadIndex].Trim('"', '\'')))
                    {
                        var potentialNextPart = originalTokens[lookAheadIndex].Trim('"', '\'');
                        var combinedPathTest = pathCandidateBuilder + " " + potentialNextPart;

                        // If the combined string looks like a path and either it resolves,
                        // or the current builder doesn't resolve and the next part isn't a path itself.
                        if (LooksLikePath(combinedPathTest))
                        {
                            var currentPathResolved = TryResolveAndValidateSinglePath(pathCandidateBuilder.ToString(), systemFolder, out _);
                            var combinedPathResolved = TryResolveAndValidateSinglePath(combinedPathTest, systemFolder, out _);
                            var nextPartLooksLikePath = LooksLikePath(potentialNextPart);

                            if (combinedPathResolved || (!currentPathResolved && !nextPartLooksLikePath))
                            {
                                pathCandidateBuilder.Append(' ').Append(potentialNextPart);
                                pathTokensConsumed++;
                                lookAheadIndex++;
                            }
                            else
                            {
                                break; // Stop consuming
                            }
                        }
                        else
                        {
                            break; // Stop consuming
                        }
                    }

                    var assembledPathArgument = pathCandidateBuilder.ToString();
                    currentIndex += (pathTokensConsumed - 1); // Adjust currentIndex to the last token consumed for path

                    if (ContainsPlaceholder(assembledPathArgument))
                    {
                        finalTokens.Add(QuoteIfNecessary(assembledPathArgument));
                    }
                    else if (lowerFlag == "-rompath" && assembledPathArgument.Contains(';'))
                    {
                        var subPaths = assembledPathArgument.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
                        var resolvedSubPaths = new List<string>();
                        foreach (var subPath in subPaths)
                        {
                            var trimmedSubPath = subPath.Trim();
                            if (TryResolveAndValidateSinglePath(trimmedSubPath, systemFolder, out var resolvedSingleSubPath))
                            {
                                resolvedSubPaths.Add(resolvedSingleSubPath);
                            }
                            else
                            {
                                resolvedSubPaths.Add(PathHelper.ResolveRelativeToAppDirectory(trimmedSubPath));
                                unresolvedPathsList.Add(trimmedSubPath);
                            }
                        }

                        finalTokens.Add(QuoteIfNecessary(string.Join(";", resolvedSubPaths)));
                    }
                    else
                    {
                        if (TryResolveAndValidateSinglePath(assembledPathArgument, systemFolder, out var resolvedPath))
                        {
                            finalTokens.Add(QuoteIfNecessary(resolvedPath));
                        }
                        else
                        {
                            finalTokens.Add(QuoteIfNecessary(PathHelper.ResolveRelativeToAppDirectory(assembledPathArgument)));
                            unresolvedPathsList.Add(assembledPathArgument);
                        }
                    }
                }
                // No else needed here, currentIndex is already advanced past the flag
            }
            else // Not a flag, not a placeholder. Could be a standalone path or other argument.
            {
                if (LooksLikePath(unquotedCurrentToken))
                {
                    if (TryResolveAndValidateSinglePath(unquotedCurrentToken, systemFolder, out var resolvedPath))
                    {
                        finalTokens.Add(QuoteIfNecessary(resolvedPath));
                    }
                    else
                    {
                        finalTokens.Add(QuoteIfNecessary(PathHelper.ResolveRelativeToAppDirectory(unquotedCurrentToken)));
                        unresolvedPathsList.Add(unquotedCurrentToken);
                    }
                }
                else
                {
                    finalTokens.Add(currentToken);
                }
            }

            currentIndex++; // Advance to the next token for the outer loop
        }

        return (string.Join(" ", finalTokens), unresolvedPathsList.Distinct().ToList());
    }

    private static string QuoteIfNecessary(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        if (path.Contains(' ') && !(path.StartsWith('"') && path.EndsWith('"')))
        {
            return $"\"{path}\"";
        }

        return path;
    }

    [GeneratedRegex("""(-\w+)\s+(?:"([^"]+)"|'([^']+)'|(\S+))""")]
    private static partial Regex MyRegex();
}