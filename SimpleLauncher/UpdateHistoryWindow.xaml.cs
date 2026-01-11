using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class UpdateHistoryWindow
{
    public UpdateHistoryWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        LoadWhatsNewContent();
    }

    private void LoadWhatsNewContent()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.md");

        try
        {
            var defaultContent = (string)Application.Current.TryFindResource("WhatsNewFileNotFound") ?? "# 'whatsnew.md' not found. The update history file could not be found.";
            var markdownText = File.Exists(filePath) ? File.ReadAllText(filePath) : defaultContent;

            // Parse Markdown and set as TextBlock with inlines
            ParseMarkdownToTextBlock(markdownText);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to load 'whatsnew.md'.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            HistoryTextBlock.Inlines.Clear();
            HistoryTextBlock.Inlines.Add(new Run((string)Application.Current.TryFindResource("UpdateHistoryLoadError") ?? "Error. Could not load the update history. The error has been logged."));
        }
    }

    /// <summary>
    /// Parses basic Markdown (headings #/##, bold **text**, italic *text* or _text_, links [text](url)) and populates the TextBlock.
    /// </summary>
    /// <param name="markdown">The Markdown text to parse.</param>
    private void ParseMarkdownToTextBlock(string markdown)
    {
        HistoryTextBlock.Inlines.Clear();
        var lines = markdown.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        var isFirstLine = true;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                // Add a line break for spacing
                HistoryTextBlock.Inlines.Add(new LineBreak());
                continue;
            }

            // Add line break before each line (except the first)
            if (!isFirstLine)
            {
                HistoryTextBlock.Inlines.Add(new LineBreak());
            }

            isFirstLine = false;

            // Parse headings (# Heading -> Bold, larger font Paragraph)
            if (trimmedLine.StartsWith("### ", StringComparison.Ordinal))
            {
                var headingText = trimmedLine.Substring(4).Trim();
                var headingRun = new Run(headingText) { FontSize = 14, FontWeight = FontWeights.Bold };
                HistoryTextBlock.Inlines.Add(new LineBreak());
                HistoryTextBlock.Inlines.Add(headingRun);
                HistoryTextBlock.Inlines.Add(new LineBreak());
            }
            else if (trimmedLine.StartsWith("## ", StringComparison.Ordinal))
            {
                var headingText = trimmedLine.Substring(3).Trim();
                var headingRun = new Run(headingText) { FontSize = 16, FontWeight = FontWeights.Bold };
                HistoryTextBlock.Inlines.Add(new LineBreak());
                HistoryTextBlock.Inlines.Add(headingRun);
                HistoryTextBlock.Inlines.Add(new LineBreak());
            }
            else if (trimmedLine.StartsWith("# ", StringComparison.Ordinal))
            {
                var headingText = trimmedLine.Substring(2).Trim();
                var headingRun = new Run(headingText) { FontSize = 20, FontWeight = FontWeights.ExtraBold };
                HistoryTextBlock.Inlines.Add(new LineBreak());
                HistoryTextBlock.Inlines.Add(headingRun);
                HistoryTextBlock.Inlines.Add(new LineBreak());
            }
            else
            {
                // Parse inline elements in the line (bold, italic, links)
                ParseInlineMarkdown(line);
            }
        }
    }

    private static readonly char[] Separator = new[] { '\r', '\n' };

    /// <summary>
    /// Parses inline Markdown in a line (bold **text**, italic *text* or _text_, links [text](url)) and adds formatted Inlines to the TextBlock.
    /// </summary>
    /// <param name="line">The line of text to parse.</param>
    private void ParseInlineMarkdown(string line)
    {
        var remainingText = line;

        // Combined regex to match bold, italic, and links in order
        var combinedRegex = CombinedMarkdownRegex();
        var matches = combinedRegex.Matches(remainingText);

        var lastIndex = 0;

        foreach (Match match in matches)
        {
            // Add plain text before this match
            if (match.Index > lastIndex)
            {
                var plainText = remainingText.Substring(lastIndex, match.Index - lastIndex);
                HistoryTextBlock.Inlines.Add(new Run(plainText));
            }

            // Determine what type of match this is
            if (match.Groups["bold"].Success)
            {
                // Bold text
                var boldText = match.Groups["bold"].Value;
                HistoryTextBlock.Inlines.Add(new Bold(new Run(boldText)));
            }
            else if (match.Groups["italic"].Success)
            {
                // Italic text
                var italicText = match.Groups["italic"].Value;
                HistoryTextBlock.Inlines.Add(new Italic(new Run(italicText)));
            }
            else if (match.Groups["linktext"].Success && match.Groups["url"].Success)
            {
                // Link [text](url)
                var linkText = match.Groups["linktext"].Value;
                var url = match.Groups["url"].Value;
                var hyperlink = new Hyperlink(new Bold(new Run(linkText)))
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
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to open link: {url}");
                    }

                    args.Handled = true;
                };
                HistoryTextBlock.Inlines.Add(hyperlink);
            }

            lastIndex = match.Index + match.Length;
        }

        // Add any remaining plain text
        if (lastIndex < remainingText.Length)
        {
            var plainText = remainingText.Substring(lastIndex);
            HistoryTextBlock.Inlines.Add(new Run(plainText));
        }
    }

    // Combined regex pattern for Markdown elements (bold, italic, links)
    // Order matters: links first (most specific), then bold, then italic
    [GeneratedRegex(@"\[(?<linktext>[^\]]+?)\]\((?<url>https?://\S+?)\)|\*\*(?<bold>.+?)\*\*|(?<delim>\*|_)(?<italic>.+?)\k<delim>", RegexOptions.Singleline)]
    private static partial Regex CombinedMarkdownRegex();
}