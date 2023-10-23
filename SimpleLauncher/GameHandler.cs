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
        public async Task<List<string>> GetFilesAsync()
        {
            return await Task.Run(() =>
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var fileExtensions = new[] { "*.zip", "*.7z", "*.iso", "*.chd", "*.cso" };
                return fileExtensions.SelectMany(ext => Directory.GetFiles(currentDirectory, ext)).ToList();
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
    }
}

