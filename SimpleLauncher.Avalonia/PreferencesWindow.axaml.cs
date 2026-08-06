using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.Services.RetroAchievements;
using SimpleLauncher.Avalonia.Views;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Preferences window with OpenEmu-style nav-strip layout.
/// RetroAchievements test connection wired to real API.
/// </summary>
public partial class PreferencesWindow : Window
{
    private readonly SettingsManagerService _settings;
    private readonly LocalizationService _localization;
    private readonly RetroAchievementsService? _raService;
    private readonly Dictionary<string, Panel> _panels = new();

    public PreferencesWindow(SettingsManagerService settings, LocalizationService localization)
    {
        InitializeComponent();
        _settings = settings;
        _localization = localization;

        try
        {
            _raService = App.ServiceProvider.GetService<RetroAchievementsService>();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "RetroAchievementsService unavailable in PreferencesWindow");
            _raService = null;
        }

        _panels["general"] = GeneralPanel;
        _panels["systems"] = SystemsPanel;
        _panels["emulators"] = EmulatorsPanel;
        _panels["images"] = ImagesPanel;
        _panels["view"] = ViewPanel;
        _panels["sound"] = SoundPanel;
        _panels["retroachievements"] = RetroAchievementsPanel;
        _panels["updates"] = UpdatesPanel;

        PopulateLanguageCombo();
        LoadSettings();
    }

    // ── Systems page: EasyMode launcher ───────────────────────────────

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OpenEasyMode_Click(object? sender, RoutedEventArgs e)
    {
        var easyModeWindow = App.ServiceProvider.GetRequiredService<EasyModeWindow>();
        easyModeWindow.ShowDialog(this);
    }

    private void OpenEditSystem_Click(object? sender, RoutedEventArgs e)
    {
        // Factory allows a pre-selected system name to be passed (null = no pre-selection)
        var editSystemWindow = App.ServiceProvider.GetRequiredService<Func<string?, EditSystemWindow>>()(null);
        editSystemWindow.ShowDialog(this);
    }

    private void PopulateLanguageCombo()
    {
        LanguageCombo.Items.Clear();
        foreach (var (code, name) in LocalizationService.AvailableLanguages)
        {
            LanguageCombo.Items.Add(new ComboBoxItem { Content = name, Tag = code });
        }
    }

    private void NavList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NavList.SelectedItem is not ListBoxItem item) return;

        var tag = item.Tag as string ?? "general";

        foreach (var panel in _panels.Values)
        {
            panel.IsVisible = false;
        }

        if (_panels.TryGetValue(tag, out var selected))
        {
            selected.IsVisible = true;
        }
    }

    private void LoadSettings()
    {
        // General
        foreach (var lbi in LanguageCombo.Items.OfType<ComboBoxItem>())
        {
            if (lbi.Tag as string == _settings.Language)
            {
                LanguageCombo.SelectedItem = lbi;
                break;
            }
        }

        // View
        foreach (var lbi in DefaultViewCombo.Items.OfType<ComboBoxItem>())
        {
            if (lbi.Tag as string == _settings.ViewMode)
            {
                DefaultViewCombo.SelectedItem = lbi;
                break;
            }
        }

        CardWidthBox.Text = _settings.ThumbnailSize.ToString();
        GamepadNavCheck.IsChecked = _settings.EnableGamePadNavigation;
        DisplayMachineNameCheck.IsChecked = _settings.DisplayMachineName;

        // Sound
        NotificationSoundCheck.IsChecked = _settings.EnableNotificationSound;

        // Images
        ImageExtensionsBox.Text = ".png, .jpg, .jpeg";

        // RA
        RaUsernameBox.Text = _settings.RaUsername ?? "";
        RaApiKeyBox.Text = _settings.RaApiKey ?? "";

        // Updates
        AutoUpdateCheck.IsChecked = true;
    }

    private void LanguageCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LanguageCombo.SelectedItem is ComboBoxItem { Tag: string lang })
        {
            _settings.Language = lang;
            _localization.LoadLanguage(lang);
        }
    }

    private async void RaTestButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var username = RaUsernameBox.Text?.Trim();
            var apiKey = RaApiKeyBox.Text?.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(apiKey))
            {
                await MessageDialogWindow.ShowAsync(this,
                    "Please enter both username and API key.",
                    "RetroAchievements",
                    MessageButtons.Ok,
                    MessageIcon.Warning);
                return;
            }

            _settings.RaUsername = username;
            _settings.RaApiKey = apiKey;

            RaTestButton.IsEnabled = false;
            RaTestButton.Content = "Testing...";

            try
            {
                if (_raService is not null)
                {
                    var profile = await _raService.GetUserProfileAsync(username, apiKey);
                    if (profile is not null)
                    {
                        await MessageDialogWindow.ShowAsync(this,
                            $"Connected as: {profile.User}\n" +
                            $"Points: {profile.TotalPoints:N0}\n" +
                            $"Rank: {profile.Rank}",
                            "RetroAchievements — Connected",
                            MessageButtons.Ok,
                            MessageIcon.Information);
                        return;
                    }
                }

                await MessageDialogWindow.ShowAsync(this,
                    "Could not connect. Check your username and API key.",
                    "RetroAchievements — Failed",
                    MessageButtons.Ok,
                    MessageIcon.Warning);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "RetroAchievements test connection failed for user {User}", username);
                await MessageDialogWindow.ShowAsync(this,
                    $"Connection error: {ex.Message}",
                    "RetroAchievements — Error",
                    MessageButtons.Ok,
                    MessageIcon.Error);
            }
            finally
            {
                RaTestButton.IsEnabled = true;
                RaTestButton.Content = "Test Connection";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "RetroAchievements test connection failed");
        }
    }
}
