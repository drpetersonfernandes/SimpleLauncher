using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the FlashOverlayWindow.
/// </summary>
public class FlashOverlayViewModel : ObservableObject
{
    private double _opacity;

    /// <summary>
    /// Gets or sets the opacity of the flash rectangle.
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    /// <summary>
    /// Event raised when the window should be closed.
    /// </summary>
    public event Action CloseRequested;

    /// <summary>
    /// Completes the flash animation and requests window close.
    /// </summary>
    public void OnAnimationCompleted()
    {
        CloseRequested?.Invoke();
    }
}
