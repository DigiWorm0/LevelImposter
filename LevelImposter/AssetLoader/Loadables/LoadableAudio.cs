using LevelImposter.Core;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableAudio(string id, IDataStore dataStore) : ICachable
{
    public string ID => id;
    public IDataStore DataStore => dataStore;
}