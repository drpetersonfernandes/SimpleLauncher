namespace SimpleLauncher.ResourceTranslator.Models;

public class MissingKeyBatch
{
    public string FilePath { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public List<KeyValuePair<string, string>> MissingKeys { get; set; } = [];
    public List<string> DuplicateKeysRemoved { get; set; } = [];
}
