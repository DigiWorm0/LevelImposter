using System;
using System.IO;
using UnityEngine;
using LevelImposter.Core;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to manage cached files in the local filesystem
    /// </summary>
    public abstract class FileCache : MonoBehaviour
    {
        public FileCache(IntPtr intPtr) : base(intPtr)
        {
        }

        private string _fileExtension = "";

        /// <summary>
        /// Sets the file extension used by the file cache
        /// </summary>
        /// <param name="fileExtension">File extension to use (including the ".")</param>
        protected void SetFileExtension(string fileExtension)
        {
            _fileExtension = fileExtension;
        }

        /// <summary>
        /// Gets the current directory where LevelImposter cached files are stored.
        /// Usually in a sub-directory within the LevelImposter folder beside the LevelImposter.dll.
        /// </summary>
        /// <returns>String path where LevelImposter map thumbnails is stored.</returns>
        private static string GetDirectory()
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
        /// Gets the name of a file based on it's ID
        /// </summary>
        /// <param name="id">File or map ID</param>
        /// <returns>Name of the file with its file extension</returns>
        private string GetFileName(string id)
        {
            return id + _fileExtension;
        }

        /// <summary>
        /// Gets the path where a specific cached file is stored.
        /// </summary>
        /// <param name="id">ID of the file</param>
        /// <returns>The path where a specific cached file is stored</returns>
        protected string GetPath(string id)
        {
            return Path.Combine(GetDirectory(), GetFileName(id));
        }

        /// <summary>
        /// Checks the existance of a cached file based on ID
        /// </summary>
        /// <param name="id">ID of the file</param>
        /// <returns>True if a file with the cooresponding ID exists</returns>
        public bool Exists(string id)
        {
            return File.Exists(GetPath(id));
        }

        /// <summary>
        /// Reads and parses a entire cached file into memory.
        /// </summary>
        /// <param name="id">ID of the file</param>
        /// <returns>Byte array with cooresponding file info</returns>
        [HideFromIl2Cpp]
        protected byte[]? Get(string id)
        {
            if (!Exists(id))
            {
                LILogger.Warn($"Could not find {GetFileName(id)} in file cache");
                return null;
            }

            return File.ReadAllBytes(GetPath(id));
        }

        /// <summary>
        /// Saves a cached file to the local filesystem.
        /// </summary>
        /// <param name="id">ID of the file</param>
        /// <param name="fileBytes">Raw data to write to disk</param>
        [HideFromIl2Cpp]
        public void Save(string id, byte[] fileBytes)
        {
            LILogger.Info($"Saving {GetFileName(id)} to file cache");
            try
            {
                string filePath = GetPath(id);
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
        /// Saves a cached file to the local filesystem.
        /// </summary>
        /// <param name="id">ID of the file</param>
        /// <param name="fileText">Raw text to write to disk</param>
        [HideFromIl2Cpp]
        public void Save(string id, string fileText)
        {
            LILogger.Info($"Saving {GetFileName(id)} to file cache");
            try
            {
                string filePath = GetPath(id);
                if (!Directory.Exists(GetDirectory()))
                    Directory.CreateDirectory(GetDirectory());
                File.WriteAllText(filePath, fileText);
            }
            catch (Exception e)
            {
                LILogger.Error(e);
            }
        }

        public void Start()
        {
            Clear(1024 * 1024 * 50); // 50 MB
            if (!Directory.Exists(GetDirectory()))
                Directory.CreateDirectory(GetDirectory());
        }
    }
}