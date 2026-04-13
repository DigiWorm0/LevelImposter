using LevelImposter.Core;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableTexture(string id, IDataStore dataStore) : IIdentifiable
{
    public string ID => id;
    public IDataStore DataStore => dataStore;
    public TextureOptions Options { get; } = new();

    public class TextureOptions
    {
        /// If true, the texture will use pixel art filtering (point filtering)
        public bool PixelArt { get; set; } = false;

        /// Changes how and when the texture is disposed of. 
        /// <c>null</c> will use <see cref="GCHandler"/>'s current default behavior.
        public GCBehavior? GCBehavior { get; set; } = null;
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