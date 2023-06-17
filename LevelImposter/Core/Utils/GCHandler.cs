using System;
using System.Collections.Generic;

namespace LevelImposter.Core
{
    /// <summary>
    /// Handles garbage collection of disposable objects
    /// </summary>
    public static class GCHandler
    {
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
    }
}
