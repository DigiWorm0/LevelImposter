using System;
using System.Collections.Generic;
using System.Diagnostics;
using LevelImposter.AssetLoader;
using Object = UnityEngine.Object;

namespace LevelImposter.Core;

/// <summary>
///     Handles garbage collection of disposable objects
/// </summary>
public static class GCHandler
{
    private static readonly Stack<IDisposable> Disposables = new();

    /// <summary>
    ///     Registers a new disposable object to be cleaned. Cleaning happens when a map is unloaded.
    /// </summary>
    /// <param name="disposable">Object to be cleaned</param>
    public static void Register(IDisposable disposable)
    {
        Disposables.Push(disposable);
    }

    /// <summary>
    ///     Creates and registers a new disposable object to be cleaned. Cleaning happens when a map is unloaded.
    /// </summary>
    /// <param name="obj">UnityEngine Object to be cleaned</param>
    public static void Register(Object obj)
    {
        Register(new DisposableUnityObject(obj));
    }

    /// <summary>
    ///     Cleans all registered disposables. Ran on map change.
    /// </summary>
    public static void Clean()
    {
        // Disposables
        LILogger.Info($"Disposing of {Disposables.Count} objects");
        while (Disposables.Count > 0)
            Disposables.Pop().Dispose();

        // Asset Loaders
        LILogger.Info($"{TextureLoader.Instance.CacheSize} cached textures");
        LILogger.Info($"{SpriteLoader.Instance.CacheSize} cached sprites");
        LILogger.Info($"{AudioLoader.Instance.CacheSize} cached audio clips");
        TextureLoader.Instance.Clear();
        SpriteLoader.Instance.Clear();
        AudioLoader.Instance.Clear();

        // GC
        GC.Collect();
    }

    /// <summary>
    ///     A thin wrapper around UnityEngine.Object.Destroy
    /// </summary>
    private class DisposableUnityObject(Object obj) : IDisposable
    {
        public void Dispose()
        {
            Object.Destroy(obj);
        }
    }
}