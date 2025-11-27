using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace SimpleLauncher;

public partial class FlashOverlayWindow
{
    public FlashOverlayWindow()
    {
        InitializeComponent();
    }

    public async Task ShowFlashAsync()
    {
        // Set the window size and position
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;

        // Create a fade-in animation
        var fadeInAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)))
        {
            AutoReverse = true // Automatically fade out
        };

        // Apply the animation to the rectangle
        FlashRectangle.BeginAnimation(OpacityProperty, fadeInAnimation);

        // Show the window
        Show();

        // Wait for the animation to complete
        await Task.Delay(600);

        // Close the window after the flash
        Close();
    }
}