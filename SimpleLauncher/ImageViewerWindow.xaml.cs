using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SimpleLauncher;

public partial class ImageViewerWindow
{
    public ImageViewerWindow()
    {
        InitializeComponent();
            
        App.ApplyThemeToWindow(this);
    }

    public void LoadImage(string imagePath)
    {
        try
        {
            byte[] imageData = System.IO.File.ReadAllBytes(imagePath);
            using (var ms = new System.IO.MemoryStream(imageData))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze(); // Freeze the bitmap to make it cross-thread accessible

                ImageViewer.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            string contextMessage = $"Failed to load the image in the Image Viewer window.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"Failed to load the image in the Image Viewer window.\n\n" +
                            $"The image may be corrupted or inaccessible." +
                            $"The error was reported to the developer that will fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}