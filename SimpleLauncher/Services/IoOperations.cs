using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class IoOperations
{
    public static void FileCopy(string source, string destination, bool overwrite)
    {
        try
        {
            File.Copy(source, destination, overwrite);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed to copy file: {source}");
        }
    }

    public static void FileMove(string source, string destination, bool overwrite)
    {
        try
        {
            File.Move(source, destination, overwrite);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed to move file: {source}");
        }
    }

    public static void CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed to create directory: {path}");
        }
    }
}