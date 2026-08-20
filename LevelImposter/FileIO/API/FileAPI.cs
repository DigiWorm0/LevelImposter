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
    private const string LEVELIMPOSTER_FOLDER_NAME = "LevelImposter";

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
        return Path.Combine(dataDirectory, LEVELIMPOSTER_FOLDER_NAME, subfolderName);
    }

    /// <summary>
    ///     Opens the file explorer to the LevelImposter data files.
    ///     For Starlight, this opens the device's built-in Files app to the starlight root.
    ///     For Desktop, this opens File Explorer (or equivalent on MacOS/Linux) to the provided subfolderName.
    /// </summary>
    /// <param name="subfolderName">Name of the subfolder to open. Keep empty to open the root LevelImposter folder.</param>
    public static void OpenInExplorer(string subfolderName = "")
    {
        if (GameState.IsMobile)
            OpenInExplorer_Mobile(subfolderName);
        else
            OpenInExplorer_Desktop(subfolderName);
    }

    private static void OpenInExplorer_Mobile(string subfolderName = "")
    {
        var activity = AndroidActivity.GetCurrent();

        // Build rootURI based on Starlight's data directory
        using var rootUri = DocumentsContract.BuildDocumentUri(
            "com.android.externalstorage.documents",
            $"primary:Documents/StarlightData/{LEVELIMPOSTER_FOLDER_NAME}/{subfolderName}");

        // Create intent to open directory
        using var intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.AddCategory(Intent.CATEGORY_OPENABLE);
        intent.SetType("*/*");
        intent.PutExtra("android.provider.extra.INITIAL_URI", rootUri);
        intent.AddFlags(0x00000001); // FLAG_GRANT_READ_URI_PERMISSION
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