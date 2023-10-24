using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class GameHandler
    {
        public async Task<List<string>> GetFilesAsync(string directoryPath)
        {
            return await Task.Run(() =>
            {
                Console.WriteLine($"Directory Path: {directoryPath}"); // Debug line

                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine("Directory doesn't exist!"); // Debug line
                    return new List<string>();
                }

                var fileExtensions = new[] { "*.zip", "*.7z", "*.iso", "*.chd", "*.cso" };
                var foundFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(directoryPath, ext)).ToList();

                Console.WriteLine($"Found {foundFiles.Count} files."); // Debug line
                return foundFiles;
            });
        }

        public List<string> FilterFiles(List<string> files, string startLetter)
        {
            if (string.IsNullOrEmpty(startLetter))
                return files; // If no startLetter is provided, no filtering is required

            if (startLetter == "#")
            {
                return files.Where(file => char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }
            else
            {
                return files.Where(file => Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        public async Task<List<string>> LoadGamesAsync(string systemDirectory)
        {
            var gameFiles = await GetFilesAsync(systemDirectory);
            var filteredGames = FilterFiles(gameFiles, "A"); // Filters games to only those starting with the letter "A"
            return filteredGames;
        }
    }
}
