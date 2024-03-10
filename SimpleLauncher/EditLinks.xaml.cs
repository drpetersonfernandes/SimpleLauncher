using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SimpleLauncher
{
    public partial class EditLinks
    {
        private readonly string _filePath = "settings.xml";
        private string VideoUrl { get; set; }
        private string InfoUrl { get; set; }
        
        public EditLinks()
        {
            InitializeComponent();
            LoadLinks();
            this.Closing += EditLinks_Closing; // attach event handler
        }

        private void LoadLinks()
        {
            if (!File.Exists(_filePath))
            {
                SetDefaultsAndSave();
                return;
            }

            try
            {
                XElement settings = XElement.Load(_filePath);

                // Validate and assign VideoUrl
                string videoUrl = settings.Element("VideoUrl")?.Value;
                VideoUrl = !string.IsNullOrEmpty(videoUrl) ? videoUrl : "https://www.youtube.com/results?search_query=";
                VideoLinkTextBox.Text = VideoUrl!;

                // Validate and assign InfoUrl
                string infoUrl = settings.Element("InfoUrl")?.Value;
                InfoUrl = !string.IsNullOrEmpty(infoUrl) ? infoUrl : "https://www.igdb.com/search?q=";
                InfoLinkTextBox.Text = InfoUrl!;
                
            }
            catch (Exception ex)
            {
                // Handle error in loading or parsing setting.xml
                MainWindow.HandleError(ex, "Error in loading or parsing setting.xml");
                // Use defaults values in case of errors
                SetDefaultsAndSave();
            }
        }

        private void SaveLinksButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate and encode VideoUrl
            var videoUrl = string.IsNullOrWhiteSpace(VideoLinkTextBox.Text)
                ? "https://www.youtube.com/results?search_query="
                : EncodeForXml(VideoLinkTextBox.Text);

            // Validate and encode InfoUrl
            var infoUrl = string.IsNullOrWhiteSpace(InfoLinkTextBox.Text)
                ? "https://www.igdb.com/search?q="
                : EncodeForXml(InfoLinkTextBox.Text);

            // Now passing the validated and encoded values to the Save method
            Save(videoUrl, infoUrl);

            MessageBox.Show("Links saved successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void RevertLinksButton_Click(object sender, RoutedEventArgs e)
        {
            // Revert VideoUrl
            VideoLinkTextBox.Text = "https://www.youtube.com/results?search_query="; 
            var videoUrl = VideoLinkTextBox.Text;

            // Revert InfoUrl
            InfoLinkTextBox.Text = "https://www.igdb.com/search?q="; 
            var infoUrl = InfoLinkTextBox.Text;

            // Now passing the validated and encoded values to the Save method
            Save(videoUrl, infoUrl);

            MessageBox.Show("Links reverted to default values.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private string EncodeForXml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // First, replace any "&amp;" with "&" to avoid double encoding.
            string decoded = input.Replace("&amp;", "&");
    
            // Now encode for XML, including re-encoding the "&"s just decoded.
            return decoded.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private void EditLinks_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prepare the process start info
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = processModule.FileName,
                    UseShellExecute = true
                };

                // Start the new application instance
                Process.Start(startInfo);

                // Shutdown the current application instance
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }
        
        private void SetDefaultsAndSave()
        {
            string videoUrl = "https://www.youtube.com/results?search_query=";
            string infoUrl = "https://www.igdb.com/search?q=";
            
            // Load the existing XML document
            var settings = File.Exists(_filePath) ? XElement.Load(_filePath) :
                // If the file does not exist, create a new Settings element
                new XElement("Settings");

            // Update the VideoUrl element, creating it if necessary
            var videoUrlElement = settings.Element("VideoUrl");
            if (videoUrlElement != null)
            {
                videoUrlElement.Value = videoUrl;
            }
            else
            {
                settings.Add(new XElement("VideoUrl", videoUrl));
            }

            // Update the InfoUrl element, creating it if necessary
            var infoUrlElement = settings.Element("InfoUrl");
            if (infoUrlElement != null)
            {
                infoUrlElement.Value = infoUrl;
            }
            else
            {
                settings.Add(new XElement("InfoUrl", infoUrl));
            }

            // Save the updated document back to the file
            settings.Save(_filePath);
        }

        private void Save(string videoUrl, string infoUrl)
        {
            // Load the existing XML document
            var settings = File.Exists(_filePath) ? XElement.Load(_filePath) :
                // If the file does not exist, create a new Settings element
                new XElement("Settings");

            // Update the VideoUrl element, creating it if necessary
            var videoUrlElement = settings.Element("VideoUrl");
            if (videoUrlElement != null)
            {
                videoUrlElement.Value = videoUrl;
            }
            else
            {
                settings.Add(new XElement("VideoUrl", videoUrl));
            }

            // Update the InfoUrl element, creating it if necessary
            var infoUrlElement = settings.Element("InfoUrl");
            if (infoUrlElement != null)
            {
                infoUrlElement.Value = infoUrl;
            }
            else
            {
                settings.Add(new XElement("InfoUrl", infoUrl));
            }

            // Save the updated document back to the file
            settings.Save(_filePath);
        }

    }
}