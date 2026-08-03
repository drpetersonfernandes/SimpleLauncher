using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.New.Services;

namespace SimpleLauncher.New.Converters;

/// <summary>
/// Converts (cardWidth, systemName, isMixedView) → card height using SystemArtRatioService.
/// Used in MultiBinding for the game card DataTemplate.
/// </summary>
public class ConsoleToCardHeightConverter : IMultiValueConverter
{
    private static SystemArtRatioService? _ratioService;

    public static void SetRatioService(SystemArtRatioService service)
    {
        _ratioService = service;
    }

    private static SystemArtRatioService? GetRatioService()
    {
        // Fast path: set by MainWindow during construction.
        if (_ratioService is not null) return _ratioService;

        // Fallback: resolve the DI singleton on first use so the converter never
        // silently produces wrong heights if it is used before MainWindow init.
        _ratioService = App.ServiceProvider?.GetService<SystemArtRatioService>();
        return _ratioService;
    }

    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2) return 168.0;

        var cardWidth = values[0] as double? ?? 168.0;
        var systemName = values[1] as string ?? "";
        var isMixedView = values.Length > 2 && values[2] is true;

        var ratioService = GetRatioService();
        if (ratioService is null) return cardWidth * 0.73;

        var artHeight = ratioService.GetArtHeight(cardWidth, systemName, isMixedView);
        return artHeight + 48; // 48px for caption area (title + rating)
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
