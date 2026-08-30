using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using SimpleLauncher.Avalonia.Services;

namespace SimpleLauncher.Avalonia.Controls;

/// <summary>
///     An <see cref="Image" /> that loads its bitmap asynchronously from a URL.
///     Binds a string URL (e.g. "https://retroachievements.org/...") to <see cref="Url" />.
///     Used in data templates (DataGrid cells, list items) where a converter cannot refresh.
/// </summary>
public class RemoteImage : Image
{
    /// <summary>
    ///     Defines the <see cref="Url" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> UrlProperty =
        AvaloniaProperty.Register<RemoteImage, string?>(nameof(Url));

    static RemoteImage()
    {
        UrlProperty.Changed.AddClassHandler<RemoteImage>(static (image, e) => image.OnUrlChanged(e));
    }

    /// <summary>
    ///     Gets or sets the image URL to load.
    /// </summary>
    public string? Url
    {
        get => GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    private void OnUrlChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var url = e.GetNewValue<string?>();
        Source = RemoteImageLoader.GetCached(url);

        if (string.IsNullOrWhiteSpace(url))
            return;

        _ = LoadAsync(url);
    }

    private async Task LoadAsync(string url)
    {
        var bitmap = await RemoteImageLoader.LoadAsync(url);

        // Only apply if the URL is still the current one (fast scrolling / reuse).
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (string.Equals(Url, url, StringComparison.OrdinalIgnoreCase)) Source = bitmap;
        });
    }
}