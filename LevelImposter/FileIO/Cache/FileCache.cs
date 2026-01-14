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

        // Create cache directory if it doesn't exist
        if (!Directory.Exists(GetDirectory()))
            Directory.CreateDirectory(GetDirectory());

        // Delete legacy directory if it exists
        DeleteLegacyDir();
    }


    /// <summary>
    /// Finds and deletes the old thumbnail directory
    /// </summary>
    private static void DeleteLegacyDir()
    {
        var gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
        var legacyDir = Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter/Thumbnails");
        if (Directory.Exists(legacyDir))
            Directory.Delete(legacyDir, true);
    }

    /// <summary>
    /// Gets the current directory where LevelImposter cached files are stored.
    /// Usually in a sub-directory within the LevelImposter folder beside the LevelImposter.dll.
    /// </summary>
    /// <returns>String path where LevelImposter map thumbnails is stored.</returns>
    public static string GetDirectory()
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

    /// <summary>
    /// Reads and parses an entire cached file into memory.
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>Byte array with cooresponding file info</returns>
    [HideFromIl2Cpp]
    public static MemoryBlock? Get(string fileName)
    {
        if (!Exists(fileName))
        {
            LILogger.Warn($"Could not find {fileName} in file cache");
            return null;
        }

        using var stream = File.OpenRead(GetPath(fileName));
        return stream.ToIl2CppArray();
    }
}