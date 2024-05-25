using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LevelImposter.Core
{
    /// <summary>
    /// Handles garbage collection of disposable objects
    /// </summary>
    public static class GCHandler
    {
        private const long MAX_MEMORY = (long)(1024 * 1024 * 1024); // 1GB
        private static Stack<IDisposable> _disposables = new();

        /// <summary>
        /// Registers a new disposable object to be cleaned. Cleaning happens when a map is unloaded.
        /// </summary>
        /// <param name="disposable">Object to be cleaned</param>
        public static void Register(IDisposable disposable)
        {
            _disposables.Push(disposable);
        }

        /// <summary>
        /// Creates and registers a new disposable object to be cleaned. Cleaning happens when a map is unloaded.
        /// </summary>
        /// <param name="obj">UnityEngine Object to be cleaned</param>
        public static void Register(UnityEngine.Object obj)
        {
            Register(new DisposableUnityObject(obj));
        }

        /// <summary>
        /// Cleans all registered disposables. Ran on map change.
        /// </summary>
        public static void Clean()
        {
            // Disposables
            LILogger.Info($"Disposing of {_disposables.Count} objects");
            while (_disposables.Count > 0)
                _disposables.Pop().Dispose();

            // SpriteLoader
            SpriteLoader.Instance?.Clean();

            // GC
            GC.Collect();
        }

        /// <summary>
        /// Gets f the current memory usage is high
        /// </summary>
        /// <returns>True if memory usage is high. False otherwise</returns>
        public static bool IsLowMemory()
        {
            Process process = Process.GetCurrentProcess();
            bool isLow = process.PrivateMemorySize64 > MAX_MEMORY;
            //if (isLow)
            //    LILogger.Msg("Warning: Low on memory");
            return isLow;
        }

        /// <summary>
        /// A thin wrapper around UnityEngine.Object.Destroy
        /// </summary>
        public class DisposableUnityObject : IDisposable
        {
            private UnityEngine.Object _obj;

            public DisposableUnityObject(UnityEngine.Object obj)
            {
                _obj = obj;
            }

            public void Dispose()
            {
                UnityEngine.Object.Destroy(_obj);
            }
        }
    }
}
