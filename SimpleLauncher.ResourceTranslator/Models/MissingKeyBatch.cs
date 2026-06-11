namespace SimpleLauncher.ResourceTranslator.Models;

public class MissingKeyBatch
{
    public string FilePath { get; set; } = "";
    public string LanguageCode { get; set; } = "";
    public string LanguageName { get; set; } = "";
    public List<KeyValuePair<string, string>> MissingKeys { get; set; } = [];
    public List<string> DuplicateKeysRemoved { get; set; } = [];
}
