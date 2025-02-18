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
        
        // Load Config
        _settingsConfig = settingsConfig;
        LoadLinks();
        
        Closing += EditLinks_Closing; 
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
        
        // Notify user
        MessageBoxLibrary.LinksSavedMessageBox();
    }

    private void RevertLinksButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsConfig.VideoUrl = "https://www.youtube.com/results?search_query=";
        _settingsConfig.InfoUrl = "https://www.igdb.com/search?q=";

        VideoLinkTextBox.Text = _settingsConfig.VideoUrl;
        InfoLinkTextBox.Text = _settingsConfig.InfoUrl;

        _settingsConfig.Save();

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

    private static void EditLinks_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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