using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SimpleLauncher
{
    public static class HelpUser
    {
        public static void UpdateHelpUserRichTextBox(
            RichTextBox helpUserRichTextBox,
            IEnumerable<TextBox> emulatorLocationTextBoxes,
            IEnumerable<TextBox> emulatorNameTextBoxes)
        {
            // if (helpUserRichTextBox == null)
            // {
            //     throw new ArgumentNullException(nameof(helpUserRichTextBox), "The help user RichTextBox cannot be null.");
            // }
            //
            // if (emulatorLocationTextBoxes == null || emulatorNameTextBoxes == null)
            // {
            //     throw new ArgumentNullException("TextBox collections cannot be null.");
            // }

            // Define target words and corresponding responses with formatting
            var emulatorResponses = new Dictionary<string, Func<string, List<Inline>>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "retroarch", retroarchFolder =>
                    [
                        new Run("Looks like you will use the ") { FontWeight = FontWeights.Normal },
                        new Run("Retroarch") { FontWeight = FontWeights.Bold },
                        new Run(" emulator") { FontWeight = FontWeights.Normal },
                        new LineBreak(),
                        new LineBreak(),
                        new Run("This emulator should have the following parameters:")
                            { FontWeight = FontWeights.Normal },
                        new LineBreak(),
                        new LineBreak(),
                        new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                        new Run($"{retroarchFolder}\\retroarch.exe") { FontWeight = FontWeights.Normal },
                        new LineBreak(),
                        new LineBreak(),
                        new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                        new Run($"-L \"{retroarchFolder}\\cores\\cap32_libretro.dll\" -f")
                            { FontWeight = FontWeights.Normal }
                    ]
                }
                // Add more emulators as needed
            };

            // Get all emulator inputs (locations and names)
            var emulatorInputs = emulatorLocationTextBoxes
                .Concat(emulatorNameTextBoxes)
                .Select(textBox => textBox?.Text)
                .Where(input => !string.IsNullOrEmpty(input))
                .Select(NormalizePath)
                .ToList();

            // Iterate through the target words and search for matches
            foreach (var emulatorResponse in emulatorResponses)
            {
                string targetWord = emulatorResponse.Key;
                var matchedInput = emulatorInputs
                    .FirstOrDefault(input =>
                        input.IndexOf(targetWord, StringComparison.OrdinalIgnoreCase) >= 0);

                if (!string.IsNullOrEmpty(matchedInput))
                {
                    // Extract the folder path if the matched input is a file path
                    var folderPath = Path.GetDirectoryName(matchedInput) ?? matchedInput;

                    // Clear existing content and add formatted text
                    helpUserRichTextBox.Document.Blocks.Clear();

                    var paragraph = new Paragraph();
                    paragraph.Inlines.AddRange(emulatorResponse.Value(folderPath));

                    helpUserRichTextBox.Document.Blocks.Add(paragraph);
                    return;
                }
            }

            // If no matches are found, display a default message
            helpUserRichTextBox.Document.Blocks.Clear();
            helpUserRichTextBox.Document.Blocks.Add(
                new Paragraph(new Run("No known emulator detected.") { FontWeight = FontWeights.Normal }));
        }

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
}
