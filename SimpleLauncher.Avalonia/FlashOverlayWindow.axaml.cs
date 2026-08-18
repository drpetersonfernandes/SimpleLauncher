using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Full-screen overlay window that displays a brief flash animation effect.
/// </summary>
public partial class FlashOverlayWindow : Window, IDisposable
{
    private readonly FlashOverlayViewModel _viewModel;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlashOverlayWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing flash overlay logic.</param>
    public FlashOverlayWindow(FlashOverlayViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.CloseRequested += (_, _) => Close();

        DataContext = _viewModel;

        Closed += (_, _) =>
        {
            _cts?.Cancel();
            Dispose();
        };
    }

    /// <summary>
    /// Displays the flash overlay with a fade-in/out animation and closes automatically.
    /// </summary>
    public async Task ShowFlashAsync()
    {
        _cts = new CancellationTokenSource();

        // Show the window before querying screens so the primary screen is available
        Show();

        // Set the window size and position to cover the primary screen
        var primaryScreen = Screens.Primary ?? Screens.All.FirstOrDefault(static s => s.IsPrimary);
        if (primaryScreen != null)
        {
            Position = primaryScreen.Bounds.Position;
            Width = primaryScreen.Bounds.Width;
            Height = primaryScreen.Bounds.Height;
        }

        // Create a fade-in/fade-out animation (0% → 100% → 0% opacity over 600ms)
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(600),
            IterationCount = new IterationCount(1),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(Visual.OpacityProperty, 0.0) } },
                new KeyFrame { Cue = new Cue(0.5), Setters = { new Setter(Visual.OpacityProperty, 1.0) } },
                new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(Visual.OpacityProperty, 0.0) } }
            }
        };

        try
        {
            await animation.RunAsync(FlashRectangle, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // Close the window after the flash
        _viewModel.OnAnimationCompleted();
    }

    /// <summary>
    /// Disposes the cancellation token source used by the flash animation and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _cts?.Dispose();
        _cts = null;
        GC.SuppressFinalize(this);
    }
}