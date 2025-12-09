using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleLauncher.Services.GameScanLogic;

/// <summary>
/// A simple parser for Valve's KeyValue (VDF) file format.
/// </summary>
public static class SteamVdfParser
{
    // Improved regex to handle escaped quotes within strings: "some \"value\" here"
    private static readonly Regex TokenRegex = new("\"((?:\\\\.|[^\\\\\"])*)\"", RegexOptions.Compiled);

    public static Dictionary<string, object> Parse(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (lines.Length == 0) return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<Dictionary<string, object>>();
            stack.Push(result);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("//", StringComparison.Ordinal) || string.IsNullOrEmpty(trimmedLine))
                    continue;

                if (trimmedLine.StartsWith('{'))
                    continue;

                if (trimmedLine.StartsWith('}'))
                {
                    if (stack.Count > 1)
                        stack.Pop();
                    continue;
                }

                var tokens = TokenRegex.Matches(trimmedLine)
                    .Select(static m => Regex.Unescape(m.Groups[1].Value))
                    .ToList();

                if (tokens.Count == 0)
                    continue;

                var currentDict = stack.Peek();

                if (tokens.Count > 1)
                {
                    var key = tokens[0];
                    var value = tokens[1];
                    currentDict[key] = value;
                }
                else
                {
                    var key = tokens[0];
                    var newDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    currentDict[key] = newDict;
                    stack.Push(newDict);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[SteamVdfParser] Failed to parse VDF file: {filePath}. Error: {ex.Message}");
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
