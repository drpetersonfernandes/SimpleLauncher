using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using Exception = System.Exception;

namespace SimpleLauncher
{
    internal class ExtractCompressedFile
    {
        private static readonly Lazy<ExtractCompressedFile> Instance = new(() => new ExtractCompressedFile());
        public static ExtractCompressedFile Instance2 => Instance.Value;
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

        public async void Cleanup()
        {
            foreach (var dir in _tempDirectories)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception exception)
                    {
                        string errorMessage = "Error occurred while cleaning up temp directories.";
                        await LogErrors.LogErrorAsync(exception, errorMessage);
                    }
                }
            }
            _tempDirectories.Clear();  // Clear the list after deleting
        }
    }
}
