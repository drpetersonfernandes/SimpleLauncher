using System.Reflection;
using System.Windows;

namespace XmlToBinaryConverter;

public partial class AboutWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        InitializeAboutContent();
    }

    private void InitializeAboutContent()
    {
        // Get version info from assembly
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        AppVersionTextBlock.Text = $"Version {version?.Major ?? 1}.{version?.Minor ?? 0}.{version?.Build ?? 0}";

        AppDescriptionTextBlock.Text = "XML to Binary Converter is a utility application that allows lossless conversion between XML files and binary DAT files using MessagePack serialization.";
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}