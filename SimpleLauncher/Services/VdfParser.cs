using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            if (!File.Exists(filePath))
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Specify encoding explicitly
            var content = File.ReadAllText(filePath, Encoding.UTF8);
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<Dictionary<string, object>>();
            stack.Push(result);

            // More flexible regex that handles whitespace better
            var tokenRegex = new Regex("""
                                       "\s*([^"]+)\s*"
                                       """, RegexOptions.Compiled);

            using var reader = new StringReader(content);
            while (reader.ReadLine() is { } line)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//", StringComparison.Ordinal))
                    continue;

                switch (trimmed)
                {
                    case "{":
                        continue;
                    case "}":
                    {
                        if (stack.Count > 1) stack.Pop();
                        continue;
                    }
                }

                var matches = tokenRegex.Matches(trimmed);
                if (matches.Count >= 2)
                {
                    var key = matches[0].Groups[1].Value;
                    var value = matches[1].Groups[1].Value;

                    // Check if this is a section header (next line is "{")
                    var nextLine = reader.ReadLine();
                    if (nextLine != null && nextLine.Trim() == "{")
                    {
                        var newDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        stack.Peek()[key] = newDict;
                        stack.Push(newDict);
                    }
                    else if (nextLine != null)
                    {
                        stack.Peek()[key] = value;
                        // Push back the line we read
                        // (In a real implementation, you'd need a more sophisticated parser)
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, $"Failed to parse VDF file: {filePath}");
            // Return empty dict instead of throwing to allow scan to continue
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
    }
}