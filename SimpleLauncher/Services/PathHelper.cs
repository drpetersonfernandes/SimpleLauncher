using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class PathHelper
{
    public static string ReturnAbsolutePath(string path)
    {
        // remove .\
        if (path.StartsWith(@".\", StringComparison.Ordinal))
        {
            path = path.Substring(2);
        }

        // convert to an absolute path
        var fullPath = Path.GetFullPath(path);

        return fullPath;
    }

    public static string SinglePathReturnAbsolutePathInsideApplicationFolderIfNeeded(string path)
    {
        // remove .\
        if (path.StartsWith(@".\", StringComparison.Ordinal))
        {
            path = path.Substring(2);
        }

        // check if it is absolute
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        // combine with the application path
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    public static string DoublePathsCombinePathsReturnAbsolutePath(string path, string path2)
    {
        // remove .\
        if (path.StartsWith(@".\", StringComparison.Ordinal))
        {
            path = path.Substring(2);
        }

        // remove .\
        if (path2.StartsWith(@".\", StringComparison.Ordinal))
        {
            path2 = path2.Substring(2);
        }

        // combine paths
        var partialPath = Path.Combine(path, path2);

        // check if it is absolute
        if (Path.IsPathRooted(partialPath))
        {
            return partialPath;
        }

        // convert to an absolute path
        var finalPath = Path.GetFullPath(partialPath);

        return finalPath;
    }
}