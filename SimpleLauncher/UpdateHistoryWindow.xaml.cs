using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class UpdateHistoryWindow
{
    public UpdateHistoryWindow()
    {
        InitializeComponent();
        LoadWhatsNewContent();
    }

    private void LoadWhatsNewContent()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.md");

        try
        {
            var markdownText = File.Exists(filePath) ? File.ReadAllText(filePath) : "# 'whatsnew.md' not found\n\nThe update history file could not be found.";

            // Parse Markdown and build FlowDocument
            var flowDocument = ParseMarkdownToFlowDocument(markdownText);

            // Set the formatted document
            HistoryTextBlock.Document = flowDocument;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to load 'whatsnew.md'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            HistoryTextBlock.Document.Blocks.Clear();
            var errorParagraph = new Paragraph { Foreground = (Brush)FindResource("MahApps.Brushes.Text") };
            errorParagraph.Inlines.Add(new Run("# Error\n\nCould not load the update history. The error has been logged."));
            HistoryTextBlock.Document.Blocks.Add(errorParagraph);
        }
    }

    /// <summary>
    /// Parses basic Markdown (headings #/##, bold **text**, italic *text* or _text_, links [text](url)) and builds a FlowDocument.
    /// </summary>
    /// <param name="markdown">The Markdown text to parse.</param>
    /// <returns>A FlowDocument with formatted content.</returns>
    private FlowDocument ParseMarkdownToFlowDocument(string markdown)
    {
        var flowDocument = new FlowDocument();
        var lines = markdown.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                // Add a blank paragraph for spacing
                flowDocument.Blocks.Add(new Paragraph());
                continue;
            }

            // Parse headings (# Heading -> Bold, larger font Paragraph)
            if (trimmedLine.StartsWith("### ", StringComparison.Ordinal))
            {
                var headingText = trimmedLine.Substring(4).Trim();
                var headingParagraph = new Paragraph(new Bold(new Run(headingText)))
                {
                    FontSize = 14, // h3 size
                    Margin = new Thickness(0, 5, 0, 10),
                    Foreground = (Brush)Application.Current.FindResource("MahApps.Brushes.Text")
                };
                flowDocument.Blocks.Add(headingParagraph);
            }
            else if (trimmedLine.StartsWith("## ", StringComparison.Ordinal))
            {
                var headingText = trimmedLine.Substring(3).Trim();
                var headingParagraph = new Paragraph(new Bold(new Run(headingText)))
                {
                    FontSize = 16, // h2 size
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5),
                    Foreground = (Brush)Application.Current.FindResource("MahApps.Brushes.Text")
                };
                flowDocument.Blocks.Add(headingParagraph);
            }
            else if (trimmedLine.StartsWith("# ", StringComparison.Ordinal))
            {
                var headingText = trimmedLine.Substring(2).Trim();
                var headingParagraph = new Paragraph(new Bold(new Run(headingText)))
                {
                    FontSize = 20, // h1 size
                    FontWeight = FontWeights.ExtraBold,
                    Margin = new Thickness(0, 20, 0, 10),
                    Foreground = (Brush)Application.Current.FindResource("MahApps.Brushes.Text")
                };
                flowDocument.Blocks.Add(headingParagraph);
            }
            else
            {
                // Parse inline elements in the line (bold, italic, links)
                var paragraph = ParseInlineMarkdown(line);
                if (paragraph.Inlines.Count > 0)
                {
                    paragraph.Foreground = (Brush)Application.Current.FindResource("MahApps.Brushes.Text");
                    flowDocument.Blocks.Add(paragraph);
                }
            }
        }

        return flowDocument;
    }

    private static readonly char[] Separator = new[] { '\r', '\n' };

    /// <summary>
    /// Parses inline Markdown in a line (bold **text**, italic *text* or _text_, links [text](url)) and returns a Paragraph with formatted Inlines.
    /// </summary>
    /// <param name="line">The line of text to parse.</param>
    /// <returns>A Paragraph with formatted Runs, Bold, Italic, and Hyperlinks.</returns>
    private Paragraph ParseInlineMarkdown(string line)
    {
        var paragraph = new Paragraph();
        var remainingText = line;

        // Parse bold **text**
        var boldMatch = BoldRegex().Match(remainingText);
        while (boldMatch.Success)
        {
            // Add text before the match
            if (boldMatch.Index > 0)
            {
                paragraph.Inlines.Add(new Run(remainingText.Substring(0, boldMatch.Index)));
            }

            // Add bold text
            var boldText = boldMatch.Groups[1].Value;
            paragraph.Inlines.Add(new Bold(new Run(boldText)));

            // Update remaining text
            remainingText = string.Concat(boldMatch.Value.AsSpan(boldMatch.Length), remainingText.AsSpan(boldMatch.Index + boldMatch.Length));
            boldMatch = BoldRegex().Match(remainingText);
        }

        // Parse italic *text* or _text_
        var italicMatch = ItalicRegex().Match(remainingText);
        while (italicMatch.Success)
        {
            // Add text before the match
            if (italicMatch.Index > 0)
            {
                paragraph.Inlines.Add(new Run(remainingText.Substring(0, italicMatch.Index)));
            }

            // Add italic text (standard Markdown italic rendering)
            var italicText = italicMatch.Groups[2].Value; // Group 2 captures the inner text
            paragraph.Inlines.Add(new Italic(new Run(italicText)));

            // Update remaining text
            remainingText = string.Concat(italicMatch.Value.AsSpan(italicMatch.Length), remainingText.AsSpan(italicMatch.Index + italicMatch.Length));
            italicMatch = ItalicRegex().Match(remainingText);
        }

        // Parse links [text](url)
        var linkMatch = MarkdownLinkRegex().Match(remainingText);
        while (linkMatch.Success)
        {
            // Add text before the match
            if (linkMatch.Index > 0)
            {
                paragraph.Inlines.Add(new Run(remainingText.Substring(0, linkMatch.Index)));
            }

            // Add hyperlink
            var linkText = linkMatch.Groups["text"].Value;
            var url = linkMatch.Groups["url"].Value;
            var hyperlink = new Hyperlink(new Run(linkText))
            {
                NavigateUri = new Uri(url)
            };
            hyperlink.RequestNavigate += (sender, args) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, $"Failed to open link: {url}");
                }

                args.Handled = true;
            };
            paragraph.Inlines.Add(hyperlink);

            // Update remaining text
            remainingText = string.Concat(linkMatch.Value.AsSpan(linkMatch.Length), remainingText.AsSpan(linkMatch.Index + linkMatch.Length));
            linkMatch = MarkdownLinkRegex().Match(remainingText);
        }

        // Add any remaining plain text
        if (!string.IsNullOrEmpty(remainingText))
        {
            paragraph.Inlines.Add(new Run(remainingText));
        }

        return paragraph;
    }

    // Regex patterns for Markdown elements
    [GeneratedRegex(@"\*\*(.+?)\*\*", RegexOptions.Singleline)]
    private static partial Regex BoldRegex();

    // Fixed regex for italic: *text* or _text_ (group 1: delimiter, group 2: inner text)
    [GeneratedRegex(@"(\*|_)(.+?)\1", RegexOptions.Singleline)]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"\[(?<text>[^\]]+?)\]\((?<url>https?://\S+?)\)", RegexOptions.Singleline)]
    private static partial Regex MarkdownLinkRegex();
}
