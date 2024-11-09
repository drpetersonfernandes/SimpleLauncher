using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace SimpleLauncher;

public partial class RomHistoryWindow
{
    private readonly string _romName;
    private readonly string _systemName;
    private readonly string _searchTerm;
    private readonly string _filePath;
    private readonly SystemConfig _systemConfig;

    public RomHistoryWindow(string romName, string systemName, string searchTerm, string filePath, SystemConfig systemConfig)
    {
        InitializeComponent();
        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;
        _filePath = filePath;
        _systemConfig = systemConfig;
        LoadPreviewImage();
        LoadRomHistory();
    }
    
    private void LoadPreviewImage()
    {
        // Get the image path using the provided method
        string imagePath = GetPreviewImagePath(_filePath, _systemConfig);

        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            PreviewImage.Source = new BitmapImage(new Uri(imagePath));
        }
        else
        {
            // Optionally, set a default/fallback image or handle when no image is found
            HistoryTextBlock.Text = "No preview image available.";
        }
    }
    
    private string GetPreviewImagePath(string filePath, SystemConfig systemConfig)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string imageFolder = !string.IsNullOrEmpty(systemConfig.SystemImageFolder)
            ? systemConfig.SystemImageFolder
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemConfig.SystemName);

        string[] extensions = [".png", ".jpg", ".jpeg"];

        foreach (var extension in extensions)
        {
            string imagePath = Path.Combine(imageFolder, $"{fileNameWithoutExtension}{extension}");
            if (File.Exists(imagePath))
            {
                return imagePath;
            }
        }

        string userDefinedDefaultImagePath = Path.Combine(imageFolder, "default.png");
        if (File.Exists(userDefinedDefaultImagePath))
        {
            return userDefinedDefaultImagePath;
        }

        string globalDefaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "default.png");
        if (File.Exists(globalDefaultImagePath))
        {
            return globalDefaultImagePath;
        }

        return string.Empty;
    }

    private void LoadRomHistory()
    {
        try
        {
            // Define the path to the history.xml file
            string historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.xml");

            // Load the XML document
            XDocument doc = XDocument.Load(historyFilePath);

            // Query for the entry with the matching name
            var entry = doc.Descendants("entry")
                .FirstOrDefault(e => e.Descendants("item")
                    .Any(i => i.Attribute("name")?.Value == _romName));

            if (entry != null)
            {
                // Extract the text element content
                string historyText = entry.Element("text")?.Value;

                if (!string.IsNullOrEmpty(historyText))
                {
                    // Display the text in the window
                    HistoryTextBlock.Text = historyText;
                }
                else
                {
                    // No history text found, prompt for online search
                    PromptForOnlineSearch();
                }
            }
            else
            {
                // No matching entry found, prompt for online search
                PromptForOnlineSearch();
            }
        }
        catch (Exception ex)
        {
            string contextMessage = $"An error occurred while loading ROM history.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"An error occurred while loading ROM history.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PromptForOnlineSearch()
    {
        var result = MessageBox.Show(
            "Simple Launcher did not find a ROM history in the local database for the selected file.\n\nDo you want to search online for the ROM history?",
            "ROM History Not Found",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            OpenGoogleSearch();
            HistoryTextBlock.Text = "No ROM history found in the local database for the selected file.";
        }
        else
        {
            HistoryTextBlock.Text = "No ROM history found in the local database for the selected file.";
        }
    }
        
    private void OpenGoogleSearch()
    {
        var query = !string.IsNullOrEmpty(_searchTerm) ? $"\"{_systemName}\" \"{_searchTerm}\" history" : $"\"{_systemName}\" \"{_romName}\" history";
        string googleSearchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = googleSearchUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            string contextMessage = $"An error occurred while opening the browser.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"An error occurred while opening the browser.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    
}