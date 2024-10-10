using LevelImposter.Core;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableAudio(string _id, IStreamable _streamable) : ICachable
{
    public string ID => _id;
    public IStreamable Streamable => _streamable;
}