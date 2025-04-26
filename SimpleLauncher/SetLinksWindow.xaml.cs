using System.Windows;

namespace SimpleLauncher;

public partial class SetLinksWindow
{
    private readonly SettingsManager _settingsManager;

    public SetLinksWindow(SettingsManager settingsManager)
    {
        InitializeComponent();

        // Load Config
        _settingsManager = settingsManager;
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
            : EncodeForXml(VideoLinkTextBox.Text);

        _settingsManager.InfoUrl = string.IsNullOrWhiteSpace(InfoLinkTextBox.Text)
            ? "https://www.igdb.com/search?q="
            : EncodeForXml(InfoLinkTextBox.Text);

        _settingsManager.Save();

        // Notify user
        MessageBoxLibrary.LinksSavedMessageBox();

        Close();
    }

    private void RevertLinksButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsManager.VideoUrl = "https://www.youtube.com/results?search_query=";
        _settingsManager.InfoUrl = "https://www.igdb.com/search?q=";

        VideoLinkTextBox.Text = _settingsManager.VideoUrl;
        InfoLinkTextBox.Text = _settingsManager.InfoUrl;

        _settingsManager.Save();

        // Notify user
        MessageBoxLibrary.LinksRevertedMessageBox();

        Close();
    }

    private static string EncodeForXml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var decoded = input.Replace("&amp;", "&");

        return decoded.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}