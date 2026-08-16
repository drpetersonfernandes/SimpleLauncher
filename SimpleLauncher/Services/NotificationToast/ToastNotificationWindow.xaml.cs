using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SimpleLauncher.Services.NotificationToast;

/// <summary>
/// Non-activating overlay window that displays a toast notification in the
/// bottom-right corner of the screen and dismisses itself after a few seconds.
/// </summary>
public partial class ToastNotificationWindow : IDisposable
{
    private const int DisplayDurationMs = 6000;

    private readonly DispatcherTimer _dismissTimer;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToastNotificationWindow"/> class.
    /// </summary>
    public ToastNotificationWindow()
    {
        InitializeComponent();

        _dismissTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(DisplayDurationMs)
        };
        _dismissTimer.Tick += DismissTimer_Tick;
    }

    /// <summary>
    /// Shows the toast with the given title and message, replacing any previous toast.
    /// </summary>
    /// <param name="title">The toast title.</param>
    /// <param name="message">The toast message.</param>
    public void ShowToast(string title, string message)
    {
        if (_isDisposed || !Application.Current.Dispatcher.CheckAccess()) return;

        _dismissTimer.Stop();

        ToastTitleTextBlock.Text = title;
        ToastMessageTextBlock.Text = message;

        Show();
        UpdateLayout();

        // Position in the bottom-right corner of the primary screen's working area
        var workingArea = SystemParameters.WorkArea;
        var width = Math.Min(MaxWidth, workingArea.Width * 0.9);
        var height = Math.Min(ActualHeight, workingArea.Height * 0.9);

        Left = workingArea.Right - width - 16;
        Top = workingArea.Bottom - height - 16;

        ActivateToastAnimation();
        _dismissTimer.Start();
    }

    private void DismissTimer_Tick(object? sender, EventArgs e)
    {
        _dismissTimer.Stop();

        var fadeOut = new DoubleAnimation(Opacity, 0, new Duration(TimeSpan.FromMilliseconds(300)));
        fadeOut.Completed += (_, _) => Hide();
        BeginAnimation(OpacityProperty, fadeOut);
    }

    private void ActivateToastAnimation()
    {
        BeginAnimation(OpacityProperty, null);
        Opacity = 1;

        var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(250)));
        ToastBorder.BeginAnimation(OpacityProperty, fadeIn);
    }

    /// <summary>
    /// Disposes the dismiss timer and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _dismissTimer.Stop();
        _dismissTimer.Tick -= DismissTimer_Tick;
        Close();
        GC.SuppressFinalize(this);
    }
}