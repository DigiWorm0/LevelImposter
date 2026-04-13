using System;
using System.IO;
using System.Reflection;
using LevelImposter.Core;

namespace LevelImposter.FileIO;

public class FileAPI
{
    private static string GetAssemblyDataPath()
    {
        var assembly = Assembly.GetAssembly(typeof(LevelImposter));
        return Path.GetDirectoryName(assembly?.Location) ?? ".";
    }

    private static string GetStarlightDataPath()
    {
        return Environment.GetEnvironmentVariable("STAR_DATA_PATH") ?? ".";
    }

    /// <summary>
    ///     Gets the full filesystem path of a safe location to store
    ///     LevelImposter data files.
    ///     For Starlight, this is provided by the STAR_DATA_PATH environment variable.
    ///     For Desktop, this is the location of LevelImposter.dll (typically BepInEx/plugins).
    /// </summary>
    /// <param name="subfolderName">Name of a subfolder or filename to use. Keep empty to store in root.</param>
    /// <returns>The full filesystem path of a safe location to store LevelImposter data files.</returns>
    public static string GetPath(string subfolderName = "")
    {
        var dataDirectory = GameState.IsMobile ? GetStarlightDataPath() : GetAssemblyDataPath();
        return Path.Combine(dataDirectory, "LevelImposter", subfolderName);
    }
}