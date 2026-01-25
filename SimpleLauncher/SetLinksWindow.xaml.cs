using System;
using System.Windows;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class SetLinksWindow
{
    private readonly SettingsManager _settingsManager;

    public SetLinksWindow(SettingsManager settingsManager)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Load Config
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        LoadLinks();
    }

    private void LoadLinks()
    {
        VideoLinkTextBox.Text = _settingsManager.VideoUrl;
        InfoLinkTextBox.Text = _settingsManager.InfoUrl;
    }

    private void SaveLinksButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsManager.VideoUrl = string.IsNullOrWhiteSpace(VideoLinkTextBox.Text)
            ? "https://www.youtube.com/results?search_query="
            : VideoLinkTextBox.Text;

        _settingsManager.InfoUrl = string.IsNullOrWhiteSpace(InfoLinkTextBox.Text)
            ? "https://www.igdb.com/search?q="
            : InfoLinkTextBox.Text;

        _settingsManager.Save();

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingLinkSettings") ?? "Saving link settings...", Application.Current.MainWindow as MainWindow);

        // Notify user
        MessageBoxLibrary.LinksSavedMessageBox();

        Close();
    }

    private void RevertLinksButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsManager.VideoUrl = _settingsManager.VideoUrl = App.Configuration["Urls:YouTubeSearch"] ?? "https://www.youtube.com/results?search_query=";
        _settingsManager.InfoUrl = App.Configuration["Urls:IgdbSearch"] ?? "https://www.igdb.com/search?q=";

        VideoLinkTextBox.Text = _settingsManager.VideoUrl;
        InfoLinkTextBox.Text = _settingsManager.InfoUrl;

        _settingsManager.Save();

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("RevertingLinkSettings") ?? "Reverting link settings...", Application.Current.MainWindow as MainWindow);

        // Notify user
        MessageBoxLibrary.LinksRevertedMessageBox();

        Close();
    }
}