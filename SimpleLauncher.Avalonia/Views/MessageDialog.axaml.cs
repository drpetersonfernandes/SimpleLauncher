using Avalonia.Controls;
using Avalonia.Interactivity;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Views;

public partial class MessageDialog : Window
{
    public string Message { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = string.Empty;
    public bool ShowIcon { get; set; }
    public bool ShowOkCancel { get; set; }
    public bool ShowYesNo { get; set; }
    public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

    public MessageDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Ok;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Cancel;
        Close();
    }

    private void OnYesClick(object? sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Yes;
        Close();
    }

    private void OnNoClick(object? sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.No;
        Close();
    }

    public static string GetIconGlyph(MessageBoxImage icon) => icon switch
    {
        MessageBoxImage.Information => "\u2139",
        MessageBoxImage.Warning => "\u26A0",
        MessageBoxImage.Error => "\u2716",
        MessageBoxImage.Question => "\u2753",
        _ => string.Empty
    };
}
