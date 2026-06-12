using System.Windows;
using System.Windows.Media.Animation;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Full-screen overlay window that displays a brief flash animation effect.
/// </summary>
public partial class FlashOverlayWindow
{
    private readonly FlashOverlayViewModel _viewModel;
    private CancellationTokenSource _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlashOverlayWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing flash overlay logic.</param>
    public FlashOverlayWindow(FlashOverlayViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.CloseRequested += Close;

        DataContext = _viewModel;

        Closing += (_, _) => _cts?.Cancel();
    }

    /// <summary>
    /// Displays the flash overlay with a fade-in/out animation and closes automatically.
    /// </summary>
    public async Task ShowFlashAsync()
    {
        _cts = new CancellationTokenSource();

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

        try
        {
            // Wait for the animation to complete
            await Task.Delay(600, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // Close the window after the flash
        _viewModel.OnAnimationCompleted();
    }
}
