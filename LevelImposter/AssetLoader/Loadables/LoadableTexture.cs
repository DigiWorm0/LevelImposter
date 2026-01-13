using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableTexture(string _id, IDataStore _dataStore) : ICachable
{
    public string ID => _id;
    public IDataStore DataStore => _dataStore;
    public readonly TextureOptions Options { get; } = new();

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
        var stream = new MemoryStore(data);
        return new LoadableTexture(id, stream);
    }
}