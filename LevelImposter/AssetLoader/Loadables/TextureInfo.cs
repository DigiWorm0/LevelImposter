using LevelImposter.AssetLoader.Queue;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.FileIO.DataBlock;
using LevelImposter.FileIO.DataStores;

namespace LevelImposter.AssetLoader.Loadables;

public readonly struct TextureInfo(string id, IDataStore dataStore) : IIdentifiable
{
    public string ID => id;
    public IDataStore DataStore => dataStore;
    public TextureOptions Options { get; } = new();

    public class TextureOptions
    {
        /// If true, the texture will use pixel art filtering (point filtering)
        public bool PixelArt { get; set; }

        /// Changes how and when the texture is disposed of.
        /// <c>null</c>
        /// will use
        /// <see cref="GCHandler" />
        /// 's current default behavior.
        public GCBehavior? GCBehavior { get; set; }
    }

    /// <summary>
    ///     Creates a LoadableTexture from data stored in memory.
    /// </summary>
    /// <param name="id">Unique identifier to be used in caching.</param>
    /// <param name="data">Byte array containing the image data.</param>
    /// <returns>A LoadableTexture instance.</returns>
    public static TextureInfo FromMemory(string id, MemoryBlock data)
    {
        var store = new MemoryStore(data);
        return new TextureInfo(id, store);
    }
}