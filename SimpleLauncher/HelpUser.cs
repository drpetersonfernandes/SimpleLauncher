using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SimpleLauncher;

public static class HelpUser
{
    public static void UpdateHelpUserRichTextBox(
        RichTextBox helpUserRichTextBox,
        IEnumerable<TextBox> emulatorLocationTextBoxes,
        IEnumerable<TextBox> emulatorNameTextBoxes,
        TextBox systemFolderTextBox)
    {
        // Initialize to empty enumerable if null
        emulatorLocationTextBoxes ??= [];
        emulatorNameTextBoxes ??= [];

        // Define target words and corresponding responses with formatting
        var emulatorResponses = new Dictionary<string, Func<string, List<Inline>>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "retroarch", retroarchFolder =>
                [
                    new Run("Recommended settings for the ") { FontWeight = FontWeights.Normal },
                    new Run("Retroarch") { FontWeight = FontWeights.Bold },
                    new Run(" emulator") { FontWeight = FontWeights.Normal },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new Run($"{retroarchFolder}\\retroarch.exe") { FontWeight = FontWeights.Normal },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new Run($"-L \"{retroarchFolder}\\cores\\[REPLACE WITH THE CORE FILENAME].dll\" -f") { FontWeight = FontWeights.Normal },
                    new LineBreak(),
                    new LineBreak()
                ]
            },
            {
                "mame", mameFolder =>
                {
                    string systemFolderPath = NormalizePath(systemFolderTextBox?.Text ?? string.Empty);

                    return
                    [
                        new Run("Recommended settings for the ") { FontWeight = FontWeights.Normal },
                        new Run("MAME") { FontWeight = FontWeights.Bold },
                        new Run(" emulator") { FontWeight = FontWeights.Normal },
                        new LineBreak(),
                        new LineBreak(),
                        new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                        new Run($"{mameFolder}\\mame.exe") { FontWeight = FontWeights.Normal },
                        new LineBreak(),
                        new LineBreak(),
                        new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                        new Run($"-rompath \"{systemFolderPath}\"") { FontWeight = FontWeights.Normal },
                        new LineBreak(),
                        new LineBreak()
                    ];
                }
            }
        };

        // Get all emulator inputs (locations and names)
        var emulatorInputs = emulatorLocationTextBoxes
            .Concat(emulatorNameTextBoxes)
            .Select(textBox => textBox?.Text)
            .Where(input => !string.IsNullOrEmpty(input))
            .Select(NormalizePath)
            .ToList();

        // Clear existing content
        helpUserRichTextBox?.Document.Blocks.Clear();

        var paragraphs = new List<Paragraph>();
        var processedEmulators = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Iterate through the target words and search for matches
        foreach (var emulatorResponse in emulatorResponses)
        {
            string targetWord = emulatorResponse.Key;
            var matchedInputs = emulatorInputs
                .Where(input => input.IndexOf(targetWord, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (matchedInputs.Any() && !processedEmulators.Contains(targetWord))
            {
                // Process the first occurrence of this emulator
                var folderPath = Path.GetDirectoryName(matchedInputs.First()) ?? matchedInputs.First();
                var paragraph = new Paragraph();
                paragraph.Inlines.AddRange(emulatorResponse.Value(folderPath));
                paragraphs.Add(paragraph);

                // Mark this emulator as processed
                processedEmulators.Add(targetWord);
            }
        }

        // Add all found paragraphs to the RichTextBox
        if (paragraphs.Any())
        {
            foreach (var paragraph in paragraphs)
            {
                helpUserRichTextBox?.Document.Blocks.Add(paragraph);
            }
        }
        else
        {
            // If no matches are found, display a default message
            helpUserRichTextBox?.Document.Blocks.Add(
                new Paragraph(new Run("No known emulator detected.") { FontWeight = FontWeights.Normal }));
        }
    }

    // Method to normalize path
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        try
        {
            // Convert relative paths to absolute paths based on the current directory
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path, AppDomain.CurrentDomain.BaseDirectory);
            }
        }
        catch
        {
            // If any exception occurs, return the original string
        }

        return path;
    }
}