using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace SimpleLauncher;

public partial class EditLinksWindow
{
    private readonly SettingsManager _settingsManager;

    public EditLinksWindow(SettingsManager settingsManager)
    {
        InitializeComponent();

        // Load Config
        _settingsManager = settingsManager;
        LoadLinks();

        Closing += EditLinks_Closing;
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

    private static void EditLinks_Closing(object sender, CancelEventArgs e)
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