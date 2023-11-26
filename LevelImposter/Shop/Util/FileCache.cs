using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using System;
using System.IO;
using System.Linq;

namespace LevelImposter.Shop
{
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
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
            string legacyDir = Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter/Thumbnails");
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
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
            return Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter/.cache");
        }

        /// <summary>
        /// Deletes all cached files from the local filesystem
        /// </summary>
        /// <param name="maxDirectorySize">Only clears cache if the directory size is greater than this many bytes</param>
        public static void Clear(long maxDirectorySize = 0)
        {
            DirectoryInfo directory = new DirectoryInfo(GetDirectory());
            if (!directory.Exists)
                return;
            long directorySize = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            if (directorySize < maxDirectorySize)
                return;

            LILogger.Info("Clearing file cache");
            try
            {
                directory.Delete(true);
            }
            catch (Exception e)
            {
                LILogger.Error(e);
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
        /// Reads and parses a entire cached file into memory.
        /// </summary>
        /// <param name="id">ID of the file</param>
        /// <returns>Byte array with cooresponding file info</returns>
        [HideFromIl2Cpp]
        public static byte[]? Get(string fileName)
        {
            if (!Exists(fileName))
            {
                LILogger.Warn($"Could not find {fileName} in file cache");
                return null;
            }

            return File.ReadAllBytes(GetPath(fileName));
        }

        /// <summary>
        /// Saves a cached file to the local filesystem.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileBytes">Raw data to write to disk</param>
        [HideFromIl2Cpp]
        public static void Save(string fileName, byte[] fileBytes)
        {
            LILogger.Info($"Saving {fileName} to file cache");
            try
            {
                string filePath = GetPath(fileName);
                if (!Directory.Exists(GetDirectory()))
                    Directory.CreateDirectory(GetDirectory());
                File.WriteAllBytes(filePath, fileBytes);
            }
            catch (Exception e)
            {
                LILogger.Error(e);
            }
        }

        /// <summary>
        /// Sabes a cached file to the local filesystem.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="dataStream">Data stream to write to disk</param>
        [HideFromIl2Cpp]
        public static void Save(string fileName, Stream dataStream)
        {
            LILogger.Info($"Saving {fileName} to file cache");
            try
            {
                string filePath = GetPath(fileName);
                if (!Directory.Exists(GetDirectory()))
                    Directory.CreateDirectory(GetDirectory());
                using (FileStream fileStream = File.Create(filePath))
                {
                    dataStream.Seek(0, SeekOrigin.Begin);
                    dataStream.CopyTo(fileStream);
                }
            }
            catch (Exception e)
            {
                LILogger.Error(e);
            }
        }

        /// <summary>
        /// Saves a cached file to the local filesystem.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileText">Raw text to write to disk</param>
        [HideFromIl2Cpp]
        public static void Save(string fileName, string fileText)
        {
            LILogger.Info($"Saving {fileName} to file cache");
            try
            {
                string filePath = GetPath(fileName);
                if (!Directory.Exists(GetDirectory()))
                    Directory.CreateDirectory(GetDirectory());
                File.WriteAllText(filePath, fileText);
            }
            catch (Exception e)
            {
                LILogger.Error(e);
            }
        }
    }
}