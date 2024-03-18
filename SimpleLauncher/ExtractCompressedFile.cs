using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleLauncher
{
    internal class ExtractCompressedFile
    {
        private static readonly Lazy<ExtractCompressedFile> _instance = new(() => new ExtractCompressedFile());
        public static ExtractCompressedFile Instance2 => _instance.Value;
        private readonly List<string> _tempDirectories = [];

        private ExtractCompressedFile() { } // Private constructor to enforce singleton pattern

        public string ExtractArchiveToTemp(string archivePath)
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);

            // Keep track of the temp directory
            _tempDirectories.Add(tempDirectory);

            using var archive = ArchiveFactory.Open(archivePath);
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(tempDirectory, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }

            return tempDirectory;
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
                        // Handle exceptions if needed
                        // Log errors or ignore
                    }
                }
            }
            _tempDirectories.Clear();  // Clear the list after deleting
        }
    }
}
