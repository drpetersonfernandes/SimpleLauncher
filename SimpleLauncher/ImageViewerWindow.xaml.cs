using System;
using System.IO;
using System.Windows.Media.Imaging;
using SimpleLauncher.Services;

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
            var imageData = File.ReadAllBytes(imagePath);
            using var ms = new MemoryStream(imageData);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze(); // Freeze the bitmap to make it cross-thread accessible

            ImageViewer.Source = bitmap;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to load the image in the Image Viewer window.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ImageViewerErrorMessageBox();
        }
    }
}