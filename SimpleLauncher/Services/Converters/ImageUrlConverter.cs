using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleLauncher.Services.Converters;

/// <summary>
/// Converts an image URL string to a BitmapImage, with fallback to a placeholder if the URL is null or empty.
/// </summary>
public class ImageUrlConverter : IValueConverter
{
    private static readonly BitmapImage PlaceholderImage = CreatePlaceholderImage();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string url && !string.IsNullOrWhiteSpace(url))
        {
            try
            {
                return new BitmapImage(new Uri(url, UriKind.Absolute));
            }
            catch
            {
                // If URL is invalid, return placeholder
                return PlaceholderImage;
            }
        }

        // Return placeholder for null/empty values
        return PlaceholderImage;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported for ImageUrlConverter");
    }

    private static BitmapImage CreatePlaceholderImage()
    {
        try
        {
            // Try to load the noimage.png from resources
            return new BitmapImage(new Uri("pack://application:,,,/SimpleLauncher;component/images/noimage.png", UriKind.Absolute));
        }
        catch
        {
            // If the image doesn't exist, create a simple gray placeholder
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.LightGray, null, new System.Windows.Rect(0, 0, 32, 32));
                drawingContext.DrawLine(new Pen(Brushes.Gray, 1), new System.Windows.Point(0, 0), new System.Windows.Point(32, 32));
                drawingContext.DrawLine(new Pen(Brushes.Gray, 1), new System.Windows.Point(32, 0), new System.Windows.Point(0, 32));
            }

            var renderTarget = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);
            renderTarget.Freeze();

            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }
    }
}
