using LevelImposter.Core;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableAudio(string id, IDataStore dataStore) : IIdentifiable
{
    public string ID => id;
    public IDataStore DataStore => dataStore;
    public AudioOptions Options { get; } = new();

    public class AudioOptions
    {
        /// Changes how and when the texture is disposed of. 
        /// <c>null</c> will use <see cref="GCHandler"/>'s current default behavior.
        public GCBehavior? GCBehavior { get; set; } = null;
    }
}