using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SevenZip;
using System.IO.Compression;

namespace SimpleLauncher
{
    internal class ExtractFile
    {
        private static readonly Lazy<ExtractFile> _instance = new Lazy<ExtractFile>(() => new ExtractFile());

        public static ExtractFile Instance => _instance.Value;

        private readonly List<string> _tempDirectories = new List<string>();

        private ExtractFile() { } // Private constructor to enforce singleton pattern

        public string ExtractArchiveToTemp(string archivePath, string formatToLaunch)
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);

            // Keep track of the temp directory
            _tempDirectories.Add(tempDirectory);

            string extension = Path.GetExtension(archivePath).ToLower();
            if (extension == ".zip")
            {
                ZipFile.ExtractToDirectory(archivePath, tempDirectory);
            }
            else if (extension == ".7z")
            {
                using (var extractor = new SevenZip.SevenZipExtractor(archivePath))
                {
                    extractor.ExtractArchive(tempDirectory);
                }
            }
            else
            {
                throw new NotSupportedException($"The file format '{extension}' is not supported.");
            }

            // Look for the first file with the desired extension
            string targetFile = Directory.GetFiles(tempDirectory, $"*.{formatToLaunch}").FirstOrDefault();
            return targetFile;
        }

        public void Cleanup()
        {
            foreach (var dir in _tempDirectories)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                        // Handle exceptions if needed (for example, if files are still in use)
                        // You might want to log the errors or simply ignore.
                    }
                }
            }
            _tempDirectories.Clear();  // Clear the list after deleting
        }
    }
}
