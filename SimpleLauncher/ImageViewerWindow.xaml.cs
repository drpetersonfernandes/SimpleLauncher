using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    public partial class ImageViewerWindow
    {
        public ImageViewerWindow()
        {
            InitializeComponent();
            
            // Apply the theme to this window
            App.ApplyThemeToWindow(this);
        }

        public void LoadImage(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // Freeze the bitmap to make it cross-thread accessible

                ImageViewer.Source = bitmap;
            }
            catch (Exception ex)
            {
                string contextMessage = $"Failed to load the image in the ImageViewerWindow.\n\nException details: {ex.Message}";
                Exception exception = new(contextMessage);
                Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"Failed to load the image.\n\nThe image might be corrupted or was inaccessible.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}