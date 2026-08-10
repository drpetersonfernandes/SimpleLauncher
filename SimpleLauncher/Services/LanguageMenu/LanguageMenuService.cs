using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.QuitOrReinstall;
using Settings = SimpleLauncher.Core.Services.SettingsManager.SettingsManagerService;

namespace SimpleLauncher.Services.LanguageMenu;

/// <summary>
/// Manages the language selection menu, including changing the application language and updating menu check marks.
/// </summary>
public class LanguageMenuService
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Settings _settings;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly ILogger _logger;
    private readonly QuitSimpleLauncher _quitSimpleLauncher;
    private ILanguageMenuHost _host = null!;

    /// <summary>
    /// Maps localized menu names to language codes (the canonical set of selectable languages).
    /// </summary>
    internal static readonly Dictionary<string, string> NameToCode = new(StringComparer.Ordinal)
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

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageMenuService"/> class.
    /// </summary>
    /// <param name="playSoundEffects">The sound effects service for playing notification sounds.</param>
    /// <param name="settings">The application settings manager.</param>
    /// <param name="messageBox">The message box service for displaying dialogs.</param>
    /// <param name="logErrors">The logger instance for error logging.</param>
    /// <param name="quitSimpleLauncher">The service for restarting the application.</param>
    public LanguageMenuService(PlaySoundEffects playSoundEffects, Settings settings, IMessageBoxLibraryService messageBox, ILogger logErrors, QuitSimpleLauncher quitSimpleLauncher)
    {
        _playSoundEffects = playSoundEffects;
        _settings = settings;
        _messageBox = messageBox;
        _logger = logErrors;
        _quitSimpleLauncher = quitSimpleLauncher;
    }

    /// <summary>
    /// Initializes the language menu service with the specified host.
    /// </summary>
    /// <param name="host">The host that provides access to menu items and UI updates.</param>
    public void Initialize(ILanguageMenuHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Looks up the language code associated with the given menu item name.
    /// </summary>
    /// <param name="menuItem">The menu item to resolve.</param>
    /// <returns>The two-letter language code, or null if not found.</returns>
    public static string? GetLanguageCodeFromMenuItem(MenuItem menuItem)
    {
        return NameToCode.GetValueOrDefault(menuItem.Name);
    }

    /// <summary>
    /// Changes the application language to the specified code, saves the setting, and restarts the application.
    /// </summary>
    /// <param name="languageCode">The two-letter language code to apply.</param>
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
            _logger.Error(ex, "Error in the method ChangeLanguageAsync");
        }
    }

    /// <summary>
    /// Updates the check marks on all language menu items to reflect the currently selected language.
    /// </summary>
    /// <param name="languageCode">The two-letter language code of the currently active language.</param>
    public void SetLanguageCheckMarks(string languageCode)
    {
        foreach (var (name, code) in NameToCode)
        {
            if (_host.FindMenuItemByName(name) is { } item)
            {
                item.IsChecked = string.Equals(code, languageCode, StringComparison.Ordinal);
            }
        }
    }
}
