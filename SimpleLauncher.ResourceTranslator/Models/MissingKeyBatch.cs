namespace SimpleLauncher.ResourceTranslator.Models;

public class MissingKeyBatch
{
    public string FilePath { get; set; } = "";
    public string LanguageCode { get; set; } = "";
    public string LanguageName { get; set; } = "";
    public IList<KeyValuePair<string, string>> MissingKeys { get; set; } = [];
    public IList<string> DuplicateKeysRemoved { get; set; } = [];
}
