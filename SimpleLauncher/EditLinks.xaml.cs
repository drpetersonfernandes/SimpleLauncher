using System;
using System.Diagnostics;
using System.Windows;

namespace SimpleLauncher;

public partial class EditLinks
{
    private readonly SettingsConfig _settingsConfig;

    public EditLinks(SettingsConfig settingsConfig)
    {
        InitializeComponent();
        _settingsConfig = settingsConfig;
        LoadLinks();
        this.Closing += EditLinks_Closing; // attach event handler
    }

    private void LoadLinks()
    {
        VideoLinkTextBox.Text = _settingsConfig.VideoUrl;
        InfoLinkTextBox.Text = _settingsConfig.InfoUrl;
    }

    private void SaveLinksButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsConfig.VideoUrl = string.IsNullOrWhiteSpace(VideoLinkTextBox.Text)
            ? "https://www.youtube.com/results?search_query="
            : EncodeForXml(VideoLinkTextBox.Text);

        _settingsConfig.InfoUrl = string.IsNullOrWhiteSpace(InfoLinkTextBox.Text)
            ? "https://www.igdb.com/search?q="
            : EncodeForXml(InfoLinkTextBox.Text);

        _settingsConfig.Save();
        string linkssavedsuccessfully2 = (string)Application.Current.TryFindResource("Linkssavedsuccessfully") ?? "Links saved successfully.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(linkssavedsuccessfully2, info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RevertLinksButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsConfig.VideoUrl = "https://www.youtube.com/results?search_query=";
        _settingsConfig.InfoUrl = "https://www.igdb.com/search?q=";

        VideoLinkTextBox.Text = _settingsConfig.VideoUrl;
        InfoLinkTextBox.Text = _settingsConfig.InfoUrl;

        _settingsConfig.Save();
        string linksreverted2 = (string)Application.Current.TryFindResource("Linksrevertedtodefaultvalues") ?? "Links reverted to default values.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(linksreverted2, info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private string EncodeForXml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string decoded = input.Replace("&amp;", "&");

        return decoded.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private void EditLinks_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = processModule.FileName,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}