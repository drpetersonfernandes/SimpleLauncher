using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.QuitOrReinstall;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.LanguageMenu;

public class LanguageMenuService
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Settings _settings;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly ILogErrors _logErrors;
    private readonly QuitSimpleLauncher _quitSimpleLauncher;
    private ILanguageMenuHost _host;

    private static readonly Dictionary<string, string> NameToCode = new()
    {
        { "LanguageArabic", "ar" },
        { "LanguageBengali", "bn" },
        { "LanguageGerman", "de" },
        { "LanguageEnglish", "en" },
        { "LanguageSpanish", "es" },
        { "LanguageFrench", "fr" },
        { "LanguageHindi", "hi" },
        { "LanguageIndonesianMalay", "id" },
        { "LanguageItalian", "it" },
        { "LanguageJapanese", "ja" },
        { "LanguageKorean", "ko" },
        { "LanguageDutch", "nl" },
        { "LanguagePortugueseBr", "pt-br" },
        { "LanguageRussian", "ru" },
        { "LanguageTurkish", "tr" },
        { "LanguageUrdu", "ur" },
        { "LanguageVietnamese", "vi" },
        { "LanguageChineseSimplified", "zh-hans" }
    };

    public LanguageMenuService(PlaySoundEffects playSoundEffects, Settings settings, IMessageBoxLibraryService messageBox, ILogErrors logErrors, QuitSimpleLauncher quitSimpleLauncher)
    {
        _playSoundEffects = playSoundEffects;
        _settings = settings;
        _messageBox = messageBox;
        _logErrors = logErrors;
        _quitSimpleLauncher = quitSimpleLauncher;
    }

    public void Initialize(ILanguageMenuHost host)
    {
        _host = host;
    }

    public static string GetLanguageCodeFromMenuItem(MenuItem menuItem)
    {
        return NameToCode.GetValueOrDefault(menuItem.Name);
    }

    public async void ChangeLanguageAsync(string languageCode)
    {
        try
        {
            if (string.IsNullOrEmpty(languageCode))
                return;

            _playSoundEffects.PlayNotificationSound();
            _settings.Language = languageCode;
            SetLanguageCheckMarks(languageCode);
            _host.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ChangingLanguage") ?? "Changing language...");
            await _settings.SaveAsync();
            await _quitSimpleLauncher.RestartApplicationAsync(_messageBox);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ChangeLanguageAsync");
        }
    }

    public void SetLanguageCheckMarks(string languageCode)
    {
        foreach (var (name, code) in NameToCode)
        {
            if (_host.FindMenuItemByName(name) is { } item)
            {
                item.IsChecked = code == languageCode;
            }
        }
    }
}
