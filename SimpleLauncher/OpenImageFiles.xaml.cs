using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SimpleLauncher
{
    public partial class OpenImageFiles
    {
        public OpenImageFiles()
        {
            InitializeComponent();
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
                MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}