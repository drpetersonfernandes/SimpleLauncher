using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class DeleteFiles
{
    public static void TryDeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        try
        {
            File.Delete(filePath);
        }
        catch (Exception)
        {
            // Ignore - this is cleanup code
        }
    }
}