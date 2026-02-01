using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static partial class BlastemConfigurationService
{
    private static readonly char[] Separator = [' ', '\t'];

    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "default.cfg");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Blastem", "default.cfg");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[BlastemConfig] Created new default.cfg from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("default.cfg not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[BlastemConfig] Injecting configuration into: {configPath}");

        var updates = new Dictionary<string, string>
        {
            { "fullscreen", settings.BlastemFullscreen ? "on" : "off" },
            { "vsync", settings.BlastemVsync ? "on" : "off" },
            { "aspect", settings.BlastemAspect },
            { "scaling", settings.BlastemScaling },
            { "scanlines", settings.BlastemScanlines ? "on" : "off" },
            { "rate", settings.BlastemAudioRate.ToString(CultureInfo.InvariantCulture) },
            { "sync_source", settings.BlastemSyncSource }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;

        // Map configuration keys to their expected parent blocks for scope validation
        var keyBlocks = new Dictionary<string, string>
        {
            { "fullscreen", "video" },
            { "vsync", "video" },
            { "aspect", "video" },
            { "scaling", "video" },
            { "scanlines", "video" },
            { "rate", "audio" },
            { "sync_source", "system" }
        };

        var braceDepth = 0;
        var currentBlock = "";

        for (var i = 0; i < lines.Count; i++)
        {
            var originalLine = lines[i];
            var trimmedLine = originalLine.Trim();

            switch (trimmedLine)
            {
                // Track block scope for hierarchical config format
                case "{":
                    braceDepth++;
                    continue;
                case "}":
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        currentBlock = "";
                    }

                    continue;
                }
            }

            // Detect block start (e.g., "video {")
            if (trimmedLine.EndsWith('{') && !trimmedLine.StartsWith('#'))
            {
                braceDepth++;
                currentBlock = trimmedLine.Substring(0, trimmedLine.Length - 1).Trim();
                continue;
            }

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#')) continue;

            var parts = trimmedLine.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var key = parts[0];
            if (!updates.TryGetValue(key, out var newValue)) continue;

            // Validate scope: only update keys when inside their expected block
            if (keyBlocks.TryGetValue(key, out var expectedBlock) && currentBlock != expectedBlock)
                continue; // Key found in wrong scope (e.g., comment or user custom section), skip it

            // Preserve original indentation and trailing comments
            var indentMatch = MyRegex().Match(originalLine);
            var indent = indentMatch.Value;

            var commentIndex = originalLine.IndexOf('#');
            var comment = commentIndex >= 0 ? originalLine.Substring(commentIndex) : "";

            var newLine = $"{indent}{key} {newValue}{(string.IsNullOrEmpty(comment) ? "" : " " + comment)}";
            if (originalLine == newLine) continue;

            lines[i] = newLine;
            modified = true;
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[BlastemConfig] Injection successful.");
        }
        else
        {
            DebugLogger.Log("[BlastemConfig] No changes needed for Blastem configuration.");
        }
    }

    [GeneratedRegex(@"^\s*")]
    private static partial Regex MyRegex();
}