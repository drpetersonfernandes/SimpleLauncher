using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SimpleLauncher.ResourceTranslator.Services;

/// <summary>
///     Shared XML escaping/unescaping and regex for XAML resource entries.
/// </summary>
public static partial class XmlHelper
{
    /// <summary>
    ///     Escapes special XML characters in a string.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text.</returns>
    public static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    /// <summary>
    ///     Unescapes XML special characters in a string.
    /// </summary>
    /// <param name="text">The text to unescape.</param>
    /// <returns>The unescaped text.</returns>
    public static string UnescapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"");
    }

    /// <summary>
    ///     Returns the compiled regex for matching XAML resource entries.
    /// </summary>
    /// <returns>The compiled regex pattern.</returns>
    [SuppressMessage("Meziantou.Analyzer", "MA0023:UseRegexOptionsExplicitCapture",
        Justification = "Capturing groups are needed to extract key and value")]
    [GeneratedRegex("""<system:String\s+x:Key="([^"]+)"[^>]*>([\s\S]*?)</system:String>""", RegexOptions.None, 1000)]
    public static partial Regex EntryRegex();
}