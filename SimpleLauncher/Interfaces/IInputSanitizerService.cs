namespace SimpleLauncher.Interfaces;

public interface IInputSanitizerService
{
    bool ContainsInvalidCharacters(string name, out char[] invalidChars);
    bool ContainsInvalidPathCharacters(string path, out char[] invalidChars);
    string SanitizeFolderName(string name);
}
