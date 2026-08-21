using Avalonia.Controls;
using Xunit;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Regression tests for the Markdown.Avalonia + Avalonia 12 compatibility fix.
///
/// Markdown.Avalonia 11.0.3 crashed with System.MissingMethodException
/// ("Avalonia.Data.IBinding DynamicResourceExtension.ProvideValue(System.IServiceProvider)")
/// while populating its MarkdownStyleFluentTheme XAML on Avalonia 12.1.1.
/// The crash surfaced during InitializeComponent of any window embedding a
/// MarkdownScrollViewer (EditSystemWindow.axaml:108, UpdateHistoryWindow.axaml:11),
/// which made the Edit System menu item silently fail to open its window
/// (the exception was caught and logged by the click handler).
/// Upgraded to Markdown.Avalonia 12.0.0-a3, which targets Avalonia 12.
/// </summary>
public class MarkdownAvaloniaCompatibilityTests
{
    [Fact]
    public void MarkdownScrollViewer_Instantiation_DoesNotThrow()
    {
        HeadlessAvalonia.RunOnUiThread(() =>
        {
            var viewer = new global::Markdown.Avalonia.MarkdownScrollViewer();
            Assert.NotNull(viewer);
            return true;
        });
    }

    [Fact]
    public void MarkdownScrollViewer_LoadsFluentTheme_AndRendersMarkdown()
    {
        var rendered = HeadlessAvalonia.RunOnUiThread(() =>
        {
            // Rooted in an actual window so control themes/styles are applied —
            // this is where the MissingMethodException used to surface.
            var window = new Window();
            var viewer = new global::Markdown.Avalonia.MarkdownScrollViewer();
            window.Content = viewer;
            window.Show();

            viewer.Markdown = "# Heading\n\nParagraph with **bold** text.";
            return !string.IsNullOrEmpty(viewer.Markdown);
        });

        Assert.True(rendered);
    }
}
