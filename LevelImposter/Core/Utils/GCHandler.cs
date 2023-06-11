using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Cleans all registered disposables
        /// </summary>
        public static void Clean()
        {
            LILogger.Info($"Disposing of {_disposables.Count} objects");
            while (_disposables.Count > 0)
            {
                _disposables.Pop().Dispose();
            }
            GC.Collect();
        }
    }
}
