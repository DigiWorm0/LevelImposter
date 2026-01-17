using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableTexture(string id, IDataStore dataStore) : ICachable
{
    public string ID => id;
    public IDataStore DataStore => dataStore;
    public TextureOptions Options { get; } = new();

    public class TextureOptions
    {
        /// If true, the texture will use pixel art filtering (point filtering)
        public bool PixelArt { get; set; } = false;
        
        /// If true (default), the texture will be disposed automatically after the map is unloaded.
        /// If false, you must manage the texture's lifecycle manually.
        public bool AddToGC { get; set; } = true;
    }
    
    /// <summary>
    /// Creates a LoadableTexture from data stored in memory.
    /// </summary>
    /// <param name="id">Unique identifier to be used in caching.</param>
    /// <param name="data">Byte array containing the image data.</param>
    /// <returns>A LoadableTexture instance.</returns>
    public static LoadableTexture FromMemory(string id, MemoryBlock data)
    {
        var store = new MemoryStore(data);
        return new LoadableTexture(id, store);
    }
}