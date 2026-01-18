using System;
using System.Collections.Generic;
using System.Linq;
using LevelImposter.AssetLoader;
using Object = UnityEngine.Object;

namespace LevelImposter.Core;

/// <summary>
///     Handles garbage collection of disposable objects
/// </summary>
public static class GCHandler
{
    private static readonly List<IGCDisposable> Disposables = [];
    private static GCBehavior _defaultBehavior = GCBehavior.AlwaysDispose;
    
    /// <summary>
    /// Changes default garbage collection behavior for registered objects
    /// </summary>
    /// <param name="behavior">New default behavior</param>
    public static void SetDefaultBehavior(GCBehavior behavior = GCBehavior.AlwaysDispose)
    {
        _defaultBehavior = behavior;
    }

    /// <summary>
    ///     Registers a new <see cref="IDisposable"/> object
    /// </summary>
    /// <param name="disposable">IDisposable object to handle</param>
    /// <param name="behavior">Garbage collection behavior for this object</param>
    public static void Register(IDisposable disposable, GCBehavior? behavior = null)
    {
        Disposables.Add(new DisposableSystemObject(disposable, behavior ?? _defaultBehavior));
    }

    /// <summary>
    ///     Registers a new UnityEngine Object for destruction
    /// </summary>
    /// <param name="obj">UnityEngine object to be cleaned</param>
    /// <param name="behavior">Garbage collection behavior for this object</param>
    public static void Register(Object obj, GCBehavior? behavior = null)
    {
        Disposables.Add(new DisposableUnityObject(obj, behavior ?? _defaultBehavior));
    }

    /// <summary>
    ///     Disposes of all registered objects matching the provided behavior
    /// </summary>
    /// <param name="behaviorFlag">Behavior flag to match for disposal</param>
    public static void DisposeAll(GCBehavior behaviorFlag = GCBehavior.AlwaysDispose)
    {
        // Filter Disposables
        var toDispose = Disposables
            .Where(d => (d.Behavior & behaviorFlag) != 0)
            .ToList();
        
        // Dispose of each object
        LILogger.Info($"Disposing of {toDispose} objects with behavior: {behaviorFlag}");
        foreach (var disposable in toDispose)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                LILogger.Warn("Error disposing 1 or more objects during GC");
                LILogger.LogException(e);
            }
            Disposables.Remove(disposable);
        }

        // Invalidate all AssetLoader caches
        // TODO: Selective cache clearing based on disposed objects
        LILogger.Info($"{TextureLoader.Instance.CacheSize} cached textures");
        LILogger.Info($"{SpriteLoader.Instance.CacheSize} cached sprites");
        LILogger.Info($"{AudioLoader.Instance.CacheSize} cached audio clips");
        TextureLoader.Instance.ClearCache();
        SpriteLoader.Instance.ClearCache();
        AudioLoader.Instance.ClearCache();

        // Force a garbage collection cycle
        GC.Collect();
    }

    private interface IGCDisposable {
        public GCBehavior Behavior { get; }
        public void Dispose();
    }
    private class DisposableSystemObject(IDisposable obj, GCBehavior behavior) : IGCDisposable
    {
        public GCBehavior Behavior { get; } = behavior;
        public void Dispose() => obj.Dispose();
    }
    private class DisposableUnityObject(Object obj, GCBehavior behavior) : IGCDisposable
    {
        public GCBehavior Behavior { get; } = behavior;
        public void Dispose() => Object.Destroy(obj);
    }
}