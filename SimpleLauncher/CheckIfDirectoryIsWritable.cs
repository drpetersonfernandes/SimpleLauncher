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
            string testFile = Path.Combine(path, Guid.NewGuid().ToString() + ".tmp");

            // Attempt to create and delete the file
            using (FileStream fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
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