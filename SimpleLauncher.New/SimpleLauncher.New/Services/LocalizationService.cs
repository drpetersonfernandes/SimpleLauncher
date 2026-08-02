using System.Text.Json;

namespace SimpleLauncher.New.Services;

/// <summary>
/// JSON-based localization service. Loads strings from Resources/strings.{lang}.json.
/// Falls back to English for missing keys.
/// </summary>
public class LocalizationService
{
    private readonly Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _enFallback;

    public string CurrentLanguage { get; private set; } = "en";

    public IReadOnlyDictionary<string, string> AllStrings => _strings;

    /// <summary>
    /// Available languages with display names.
    /// </summary>
    public static readonly Dictionary<string, string> AvailableLanguages = new()
    {
        ["en"] = "English",
        ["pt-BR"] = "Português",
        ["es"] = "Español",
        ["fr"] = "Français",
        ["de"] = "Deutsch",
        ["ja"] = "日本語",
        ["ko"] = "한국어",
        ["zh-Hans"] = "简体中文",
        ["ru"] = "Русский",
        ["it"] = "Italiano",
        ["nl"] = "Nederlands",
        ["tr"] = "Türkçe"
    };

    public LocalizationService()
    {
        LoadLanguage("en");
        _enFallback = new Dictionary<string, string>(_strings, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Loads a language file. Falls back to English for missing keys.
    /// </summary>
    public void LoadLanguage(string lang)
    {
        CurrentLanguage = lang;
        _strings.Clear();

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"strings.{lang}.json");

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict is not null)
                {
                    foreach (var kvp in dict)
                    {
                        _strings[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                // Fall through to English
                Log.Error(ex, "Failed to load language file {Path}", path);
            }
        }

        // If not English, merge English fallback for missing keys
        if (lang != "en" && _enFallback.Count > 0)
        {
            foreach (var kvp in _enFallback)
            {
                _strings.TryAdd(kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// Gets a localized string by key. Returns the key itself if not found.
    /// </summary>
    public string GetString(string key)
    {
        return _strings.GetValueOrDefault(key, key);
    }

    /// <summary>
    /// Gets a formatted localized string.
    /// </summary>
    public string GetString(string key, params object[] args)
    {
        var template = GetString(key);
        return args.Length > 0 ? string.Format(template, args) : template;
    }
}
