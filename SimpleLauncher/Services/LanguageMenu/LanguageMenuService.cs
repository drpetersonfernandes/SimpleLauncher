using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.QuitOrReinstall;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.LanguageMenu;

public class LanguageMenuService
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Settings _settings;
    private MainWindow _mainWindow;

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

    public LanguageMenuService(PlaySoundEffects playSoundEffects, Settings settings)
    {
        _playSoundEffects = playSoundEffects;
        _settings = settings;
    }

    public void Initialize(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void ChangeLanguage(MenuItem menuItem)
    {
        if (!NameToCode.TryGetValue(menuItem.Name, out var languageCode))
            return;

        _playSoundEffects.PlayNotificationSound();
        _settings.Language = languageCode;
        SetLanguageCheckMarks(languageCode);
        _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ChangingLanguage") ?? "Changing language...", _mainWindow);
        _settings.SaveAsync();
        QuitSimpleLauncher.RestartApplication();
    }

    public void SetLanguageCheckMarks(string languageCode)
    {
        foreach (var (name, code) in NameToCode)
        {
            if (_mainWindow.FindName(name) is MenuItem item)
            {
                item.IsChecked = code == languageCode;
            }
        }
    }
}