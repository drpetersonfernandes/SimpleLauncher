using System.Text.Json;

namespace SimpleLauncher.Avalonia.Services;

/// <summary>
///     JSON-based localization service. Loads strings from Resources/strings.{lang}.json.
///     Falls back to English for missing keys.
/// </summary>
public class LocalizationService
{
    /// <summary>
    ///     Available languages with display names (canonical set matches the WPF app).
    /// </summary>
    public static readonly Dictionary<string, string> AvailableLanguages = new()
    {
        ["ar"] = "العربية",
        ["bn"] = "বাংলা",
        ["de"] = "Deutsch",
        ["en"] = "English",
        ["es"] = "Español",
        ["fr"] = "Français",
        ["hi"] = "हिन्दी",
        ["id"] = "Indonesian (Malay)",
        ["it"] = "Italiano",
        ["ja"] = "日本語",
        ["ko"] = "한국어",
        ["nl"] = "Nederlands",
        ["pt-BR"] = "Português",
        ["ru"] = "Русский",
        ["tr"] = "Türkçe",
        ["ur"] = "اردو",
        ["vi"] = "Tiếng Việt",
        ["zh-Hans"] = "简体中文"
    };

    private readonly Dictionary<string, string> _enFallback;
    private readonly string _resourcesDir;
    private readonly Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);

    public LocalizationService() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"))
    {
    }

    /// <summary>
    ///     Test seam: allows tests to load strings from an isolated directory instead of
    ///     mutating the shared output Resources folder (which races with LocalizationTests).
    /// </summary>
    internal LocalizationService(string resourcesDir)
    {
        _resourcesDir = resourcesDir;
        LoadLanguage("en");
        _enFallback = new Dictionary<string, string>(_strings, StringComparer.OrdinalIgnoreCase);
    }

    public string CurrentLanguage { get; private set; } = "en";

    public IReadOnlyDictionary<string, string> AllStrings => _strings;

    /// <summary>
    ///     Loads a language file. Falls back to English for missing keys.
    /// </summary>
    public void LoadLanguage(string lang)
    {
        CurrentLanguage = lang;
        _strings.Clear();

        // Resolve the resource file case-insensitively (settings may store codes
        // like 'pt-br' or 'zh-hans' from the WPF app while the files use 'pt-BR'/'zh-Hans').
        var path = Directory.Exists(_resourcesDir)
            ? Directory.EnumerateFiles(_resourcesDir, "strings.*.json")
                .FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f).Substring("strings.".Length),
                    lang, StringComparison.OrdinalIgnoreCase))
            : null;
        path ??= Path.Combine(_resourcesDir, $"strings.{lang}.json");

        if (File.Exists(path))
        {
            // Canonicalize CurrentLanguage to the actual file's code (e.g. 'pt-br' -> 'pt-BR')
            var fileName = Path.GetFileNameWithoutExtension(path);
            CurrentLanguage = fileName.StartsWith("strings.", StringComparison.Ordinal)
                ? fileName["strings.".Length..]
                : lang;
        }

        if (File.Exists(path))
            try
            {
                var json = File.ReadAllText(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict is not null)
                    foreach (var kvp in dict)
                        _strings[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                // Fall through to English
                Log.Error(ex, "Failed to load language file {Path}", path);
            }

        // If not English, merge English fallback for missing keys
        if (!string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) && _enFallback.Count > 0)
            foreach (var kvp in _enFallback)
                _strings.TryAdd(kvp.Key, kvp.Value);
    }

    /// <summary>
    ///     Gets a localized string by key. Returns the key itself if not found.
    /// </summary>
    public string GetString(string key)
    {
        return _strings.GetValueOrDefault(key, key);
    }

    /// <summary>
    ///     Gets a localized string by key, returning <paramref name="fallback" /> when the key is missing.
    /// </summary>
    public string GetString(string key, string fallback)
    {
        return _strings.GetValueOrDefault(key, fallback);
    }

    /// <summary>
    ///     Gets a formatted localized string.
    /// </summary>
    public string GetString(string key, params object[] args)
    {
        var template = GetString(key);
        return args.Length > 0 ? string.Format(template, args) : template;
    }
}