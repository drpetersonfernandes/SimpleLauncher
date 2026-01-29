using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher;

public partial class ImageViewerWindow
{
    public ImageViewerWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
    }

    public void LoadImagePath(string imagePath)
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ImageViewerErrorMessageBox();
        }
    }

    /// <summary>
    /// Loads an image from a URI (local or web) into the viewer.
    /// </summary>
    /// <param name="imageUri">The URI of the image.</param>
    public void LoadImageUrl(Uri imageUri)
    {
        try
        {
            if (imageUri != null)
            {
                ImageViewer.Source = new BitmapImage(imageUri);
            }
            else
            {
                ImageViewer.Source = null;
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load image from URI in RetroAchievementsImageViewerWindow: {imageUri}");
            ImageViewer.Source = null;
        }
    }
}