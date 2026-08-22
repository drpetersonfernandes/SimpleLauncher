using System;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleLauncher.Avalonia.Extensions;

/// <summary>
/// Resolves a localized string from the <see cref="Services.LocalizationService"/> at XAML
/// parse time (the Avalonia app requires a restart after a language change, so parse-time
/// resolution matches the runtime behavior of every other localized string).
/// Falls back to the key itself when the service or the key is unavailable.
/// </summary>
public class TranslateExtension : MarkupExtension
{
    public TranslateExtension()
    {
    }

    public TranslateExtension(string key)
    {
        Key = key;
    }

    public string? Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            return string.Empty;
        }

        var localization = App.ServiceProvider?.GetService<Services.LocalizationService>();
        return localization?.GetString(Key) ?? Key;
    }
}
