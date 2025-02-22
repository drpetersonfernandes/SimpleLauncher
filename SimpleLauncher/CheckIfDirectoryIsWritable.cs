using System;
using System.IO;

namespace SimpleLauncher;

public static class CheckIfDirectoryIsWritable
{
    public static bool IsWritableDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            // Generate a unique temporary file path
            var testFile = Path.Combine(path, Guid.NewGuid() + ".tmp");

            // Attempt to create and delete the file
            using (var fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Close();
            }

            File.Delete(testFile);

            return true;
        }
        catch
        {
            return false;
        }
    }
}