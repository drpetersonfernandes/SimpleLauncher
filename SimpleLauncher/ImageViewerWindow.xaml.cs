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
                string contextMessage = $"Failed to load the image in the Image Viewer window.\n\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show($"Failed to load the image in the Image Viewer window.\n\nThe image may be corrupted or inaccessible.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}