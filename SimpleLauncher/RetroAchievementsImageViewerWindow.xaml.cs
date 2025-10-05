using System;
using System.Windows.Media.Imaging;
using SimpleLauncher.Services;

namespace SimpleLauncher;

/// <summary>
/// A dedicated window to display RetroAchievements images with basic scrolling/zooming capabilities.
/// </summary>
public partial class RetroAchievementsImageViewerWindow
{
    public RetroAchievementsImageViewerWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this); // Apply the application theme
    }

    /// <summary>
    /// Loads an image from a URI (local or web) into the viewer.
    /// </summary>
    /// <param name="imageUri">The URI of the image.</param>
    public void LoadImage(Uri imageUri)
    {
        try
        {
            if (imageUri != null)
            {
                ImageViewerImage.Source = new BitmapImage(imageUri);
            }
            else
            {
                ImageViewerImage.Source = null;
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed to load image from URI in RetroAchievementsImageViewerWindow: {imageUri}");
            ImageViewerImage.Source = null;
        }
    }
}