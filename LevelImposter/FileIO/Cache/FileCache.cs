using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using System;
using System.IO;
using System.Linq;

namespace LevelImposter.FileIO;

/// <summary>
/// API to manage cached files in the local filesystem
/// </summary>
public static class FileCache
{
    /// <summary>
    /// Runs various initialization tasks for the FileCache
    /// </summary>
    public static void Init()
    {
        // Clear cache if it's too big
        Clear(1024 * 1024 * 50); // 50 MB

        // Ensure cache directory exists
        MakeDirectoryIfNotExists();
    }
    
    /// <summary>
    /// Makes the cache directory if it does not already exist
    /// </summary>
    private static void MakeDirectoryIfNotExists()
    {
        var directory = GetDirectory();
        if (Directory.Exists(directory))
            return;
        var cacheFolder = Directory.CreateDirectory(directory);
        cacheFolder.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
    }

    /// <summary>
    /// Gets the current directory where LevelImposter cached files are stored.
    /// Usually in a subdirectory within the LevelImposter folder beside the LevelImposter.dll.
    /// </summary>
    /// <returns>String path where LevelImposter map thumbnails is stored.</returns>
    private static string GetDirectory()
    {
        var gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
        return Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter/.cache");
    }

    /// <summary>
    /// Deletes all cached files from the local filesystem
    /// </summary>
    /// <param name="maxDirectorySize">Only clears cache if the directory size is greater than this many bytes</param>
    public static void Clear(long maxDirectorySize = 0)
    {
        // Get Directory
        var directory = new DirectoryInfo(GetDirectory());
        if (!directory.Exists)
            return;

        // Check Directory Size
        var directorySize = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        if (directorySize < maxDirectorySize)
            return;

        // Clear Directory
        try
        {
            LILogger.Info("Clearing file cache");
            directory.Delete(true);
        }
        catch (Exception e)
        {
            LILogger.Warn("Failed to clear file cache");
            LILogger.Info(e);
        }
    }

    /// <summary>
    /// Gets the path where a specific cached file is stored.
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>The path where a specific cached file is stored</returns>
    public static string GetPath(string fileName)
    {
        return Path.Combine(GetDirectory(), fileName);
    }

    /// <summary>
    /// Checks the existance of a cached file based on ID
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>True if a file with the cooresponding ID exists</returns>
    public static bool Exists(string fileName)
    {
        return File.Exists(GetPath(fileName));
    }
}