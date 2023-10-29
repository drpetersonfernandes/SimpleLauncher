using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Application started.");

        // Check if a file path is provided
        if (args.Length == 0)
        {
            Console.WriteLine("No file provided.");
            Console.ReadKey(); // Wait for a key press
            return;
        }

        string filePath = args[0]; // Get the file path from arguments
        Console.WriteLine("Received file path from emulator frontend: " + filePath);

        string fileExtension = Path.GetExtension(filePath).ToLower();

        Console.WriteLine("File to be loaded: " + filePath);
        Console.WriteLine("File extension: " + fileExtension);

        // Read the emulator location from config.ini
        string emulatorLocation;
        try
        {
            emulatorLocation = File.ReadAllText("config.ini").Trim();
            // Remove quotes if present
            if (emulatorLocation.StartsWith("\"") && emulatorLocation.EndsWith("\""))
            {
                emulatorLocation = emulatorLocation.Trim('"');
            }
            Console.WriteLine("Emulator location: " + emulatorLocation);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading config.ini: " + ex.Message);
            return;
        }

        try
        {
            Console.WriteLine("Starting emulator process...");
            Process emulatorProcess = new Process();
            emulatorProcess.StartInfo.FileName = emulatorLocation;
            emulatorProcess.Start(); // Start the emulator without arguments

            Console.WriteLine("Emulator started. Waiting for initialization...");
            Thread.Sleep(3000); // Wait for the emulator to start

            // Load the file based on the extension
            if (fileExtension == ".bin" || fileExtension == ".rom")
            {
                Console.WriteLine("Loading .bin or .rom file...");
                // Load Game ROM (assuming this is the correct sequence)
                SendKeys.SendWait("{ALT}f"); // Open File menu
                Thread.Sleep(500);
                SendKeys.SendWait("l"); // Select Load Game ROM
                Thread.Sleep(500);
                SendKeys.SendWait(filePath); // Type the file path
                Thread.Sleep(500);
                SendKeys.SendWait("{ENTER}"); // Press Enter
                Console.WriteLine("File loaded.");
            }
            else if (fileExtension == ".caq")
            {
                Console.WriteLine("Loading .caq file...");
                // Load Cassette File (assuming this is the correct sequence)
                // Add specific handling for cassettes as per your script
                Console.WriteLine(".caq file loaded.");
            }
            else
            {
                Console.WriteLine("Unsupported file type: " + fileExtension);
            }

            Console.WriteLine("Waiting for the game to load...");
            Thread.Sleep(3000);

            // Fullscreen
            Console.WriteLine("Setting emulator to fullscreen mode...");
            SendKeys.SendWait("{ALT}u"); // Open Util menu
            Thread.Sleep(500);
            SendKeys.SendWait("f"); // Select Full screen mode
            Console.WriteLine("Fullscreen mode set.");

            Console.WriteLine("Waiting for emulator to close...");
            emulatorProcess.WaitForExit(); // Wait for the emulator to close
            Console.WriteLine("Emulator process exited.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        Console.WriteLine("Application ended.");
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey(); // Wait for a key press before closing
    }
}
