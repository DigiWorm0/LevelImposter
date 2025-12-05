using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableTexture(string _id, IStreamable _streamable) : ICachable
{
    public string ID => _id;
    public IStreamable Streamable => _streamable;
    public readonly TextureOptions Options { get; } = new();

    public class TextureOptions
    {
        /// If true, the texture will use pixel art filtering (point filtering)
        public bool PixelArt { get; set; } = false;
        
        /// If false, the texture will be disposed automatically after the map is unloaded
        public bool AddToGC { get; set; } = true;
    }
    
    /// <summary>
    /// Creates a LoadableTexture from a byte array.
    /// </summary>
    /// <param name="id">Unique identifier to be used in caching.</param>
    /// <param name="data">Byte array containing the image data.</param>
    /// <returns>A LoadableTexture instance.</returns>
    public static LoadableTexture FromByteArray(string id, Il2CppArrayBase<byte> data)
    {
        var stream = new MemoryStreamable(data);
        return new LoadableTexture(id, stream);
    }
}