using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LevelImposter.Core.Android;
using LevelImposter.Core.Utils;
using AndroidActivity = LevelImposter.Core.Android.Activity;

namespace LevelImposter.FileIO.API;

public static class FileAPI
{
    private const string DATA_PATH_ENVIRONMENT_VARIABLE = "LI_DATA_PATH";

    /// <summary>
    ///     Overrides the root data directory. Intended for tests and tooling.
    ///     When null, <c>LI_DATA_PATH</c> is used, then the platform default.
    /// </summary>
    public static string? DataRootOverride { get; set; }


    /// <summary>
    ///     Gets the root data directory, honoring test overrides and <c>LI_DATA_PATH</c>.
    /// </summary>
    private static string GetDataRoot()
    {
        if (!string.IsNullOrWhiteSpace(DataRootOverride))
            return DataRootOverride;

        var environmentPath = Environment.GetEnvironmentVariable(DATA_PATH_ENVIRONMENT_VARIABLE);
        if (!string.IsNullOrWhiteSpace(environmentPath))
            return environmentPath;

        return GameState.IsMobile ? GetDataRoot_Mobile() : GetDataRoot_Desktop();
    }
    
    private static string GetDataRoot_Desktop()
    {
        var assembly = Assembly.GetAssembly(typeof(LevelImposter));
        return Path.GetDirectoryName(assembly?.Location) ?? ".";
    }

    private static string GetDataRoot_Mobile()
    {
        return Environment.GetEnvironmentVariable("STAR_DATA_PATH") ?? ".";
    }

    /// <summary>
    ///     Attempts to delete a file from the given path, ignoring any exceptions.
    ///     This is useful for cleaning up any extra files in the event of an error.
    /// </summary>
    /// <param name="path">The path of the file to delete.</param>
    public static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Ignore any exceptions
        }
    }

    /// <summary>
    ///     Gets the full filesystem path of a safe location to store LevelImposter data files.
    ///     For Starlight, this is provided by the STAR_DATA_PATH environment variable.
    ///     For Desktop, this is the location of LevelImposter.dll (typically BepInEx/plugins).
    /// </summary>
    /// <param name="subfolderName">Name of a subfolder or filename to use. Keep empty to store in root.</param>
    /// <returns>The full filesystem path of a safe location to store LevelImposter data files.</returns>
    public static string GetPath(string subfolderName = "")
    {
        return Path.Combine(GetDataRoot(), "LevelImposter", subfolderName);
    }

    /// <summary>
    ///     Opens the file explorer to the LevelImposter data files.
    ///     For Starlight, this opens the device's built-in Files app to the starlight root.
    ///     For Desktop, this opens File Explorer (or equivalent on MacOS/Linux) to the provided subfolderName.
    /// </summary>
    public static void OpenInExplorer()
    {
        if (GameState.IsMobile)
            OpenInExplorer_Mobile();
        else
            OpenInExplorer_Desktop();
    }

    private static void OpenInExplorer_Mobile()
    {
        var activity = AndroidActivity.GetCurrent();

        // Get the application package name (ex: dev.allofus.starlight)
        var packageName = activity.GetPackageName();
        if (string.IsNullOrWhiteSpace(packageName))
            throw new InvalidOperationException("Couldn't determine the current package name.");

        // Build rootURI based on Starlight's data directory
        using var rootUri = DocumentsContract.BuildRootUri(
            $"{packageName}.documents",
            "star_data");

        // Create intent to open directory
        using var intent = new Intent("android.intent.action.VIEW");
        intent.SetData(rootUri);
        intent.AddFlags(0x00000001); // <-- FLAG_GRANT_READ_URI_PERMISSION
        activity.StartActivity(intent);
    }

    private static void OpenInExplorer_Desktop(string subfolderName = "")
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = GetPath(subfolderName),
            UseShellExecute = true
        });
    }
}