using System.Text.RegularExpressions;

namespace SimpleLauncher.ResourceTranslator.Services;

/// <summary>
/// Shared XML escaping/unescaping and regex for XAML resource entries.
/// </summary>
public static partial class XmlHelper
{
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

    [GeneratedRegex("""<system:String\s+x:Key="([^"]+)"[^>]*>([\s\S]*?)</system:String>""")]
    public static partial Regex EntryRegex();
}
