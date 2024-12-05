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
        TextBox systemFolderTextBox)
    {
        // Initialize to empty enumerable if null
        emulatorLocationTextBoxes ??= [];

        string systemFolderPath = NormalizePath(systemFolderTextBox?.Text ?? string.Empty);
        
        // Define target words and corresponding responses with formatting
        var emulatorResponses = new Dictionary<string, Func<string, List<Inline>>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "retroarch", retroarchFolder =>
                [
                    new Run("Retroarch") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{retroarchFolder}\\retroarch.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-L \"{retroarchFolder}\\cores\\[REPLACE WITH DESIRED CORE FILENAME].dll\" -f"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "mame", mameFolder =>
                [
                    new Run("MAME") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{mameFolder}\\mame.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-rompath \"{systemFolderPath};{mameFolder}\\roms;{mameFolder}\\bios\""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "stella", stellaFolder =>
                [
                    new Run("Stella") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{stellaFolder}\\Stella.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-fullscreen 1"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "altirra", altirraFolder =>
                [
                    new Run("Altirra") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{altirraFolder}\\Altirra64.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"/f"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "bigpemu", bigpemuFolder =>
                [
                    new Run("BigPEmu") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{bigpemuFolder}\\BigPEmu.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "mednafen", mednafenFolder =>
                [
                    new Run("Mednafen") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{mednafenFolder}\\mednafen.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "hatari", hatariFolder =>
                [
                    new Run("Hatari") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{hatariFolder}\\hatari.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "bizhawk", bizhawkFolder =>
                [
                    new Run("BizHawk") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{bizhawkFolder}\\EmuHawk.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "emuhawk", emuhawkFolder =>
                [
                    new Run("BizHawk") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{emuhawkFolder}\\EmuHawk.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "openmsx", openmsxFolder =>
                [
                    new Run("OpenMSX") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{openmsxFolder}\\openmsx.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "msxec", msxecFolder =>
                [
                    new Run("MSXEC") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{msxecFolder}\\MSXEC.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "xemu", xemuFolder =>
                [
                    new Run("Xemu") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{xemuFolder}\\xemu.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-full-screen -dvd_path"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "xenia", xeniaFolder =>
                [
                    new Run("Xenia") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{xeniaFolder}\\xenia.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "ares", aresFolder =>
                [
                    new Run("ares") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{aresFolder}\\ares.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"--system \"[REPLACE WITH THE TYPE OF SYSTEM]\""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "citra", citraFolder =>
                [
                    new Run("Citra") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{citraFolder}\\citra-qt.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "dolphin", dolphinFolder =>
                [
                    new Run("Dolphin") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{dolphinFolder}\\Dolphin.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "snes9x", snes9XFolder =>
                [
                    new Run("Snes9x") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{snes9XFolder}\\snes9x-x64.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-fullscreen"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "ryujinx", ryujinxFolder =>
                [
                    new Run("Ryujinx") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{ryujinxFolder}\\Ryujinx.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "yuzu", yuzuFolder =>
                [
                    new Run("Yuzu") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{yuzuFolder}\\yuzu.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "cemu", cemuFolder =>
                [
                    new Run("Cemu") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{cemuFolder}\\cemu.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-f -g"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "redream", redreamFolder =>
                [
                    new Run("Redream") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{redreamFolder}\\redream.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "mastergear", mastergearFolder =>
                [
                    new Run("MasterGear") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{mastergearFolder}\\MG.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "duckstation", duckstationFolder =>
                [
                    new Run("DuckStation") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{duckstationFolder}\\duckstation-qt-x64-ReleaseLTCG.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-fullscreen"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "pcsx2", pcsx2Folder =>
                [
                    new Run("PCSX2") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{pcsx2Folder}\\pcsx2-qt.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"-fullscreen"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "ppsspp", ppssppFolder =>
                [
                    new Run("PPSSPP") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{ppssppFolder}\\PPSSPPWindows64.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"--fullscreen"),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            },
            {
                "project64", project64Folder =>
                [
                    new Run("Project 64") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Location: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($"{project64Folder}\\Project64.exe"),
                    new LineBreak(),
                    new LineBreak(),
                    new Run("Emulator Parameters: ") { FontWeight = FontWeights.Bold },
                    new LineBreak(),
                    new Run($""),
                    new LineBreak(),
                    new Run("--------------------------")
                ]
            }
            
        };

        // Get all emulator inputs (locations and names)
        var emulatorInputs = emulatorLocationTextBoxes
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
                new Paragraph(new Run("No known emulator detected.")));
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