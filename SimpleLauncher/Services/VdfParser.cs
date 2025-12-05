using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SimpleLauncher.Services;

/// <summary>
/// A simple parser for Valve's KeyValue (VDF) file format.
/// </summary>
public static class VdfParser
{
    private static readonly Regex KeyValueRegex = new("""
                                                      \"([^\"]+)\"\s*\"([^\"]+)\"
                                                      """, RegexOptions.Compiled);

    public static Dictionary<string, object> Parse(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return new Dictionary<string, object>();

            var lines = File.ReadAllLines(filePath);
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<Dictionary<string, object>>();
            stack.Push(result);

            var keyRegex = new Regex("""
                                     \"([^\"]+)\"
                                     """);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//", StringComparison.Ordinal)) continue;

                if (trimmedLine == "{") continue;

                if (trimmedLine == "}")
                {
                    if (stack.Count > 1) stack.Pop();
                    continue;
                }

                var matches = keyRegex.Matches(trimmedLine);
                if (matches.Count == 1) // It's a key for a new section
                {
                    var key = matches[0].Groups[1].Value;
                    var newDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    stack.Peek()[key] = newDict;
                    stack.Push(newDict);
                }
                else if (matches.Count == 2) // It's a key-value pair
                {
                    var key = matches[0].Groups[1].Value;
                    var value = matches[1].Groups[1].Value;
                    stack.Peek()[key] = value;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, $"Failed to parse VDF file: {filePath}");
            throw; // Rethrow to be caught and logged by the GameScannerService
        }
    }
}