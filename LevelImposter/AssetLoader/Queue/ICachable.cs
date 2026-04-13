namespace LevelImposter.AssetLoader;

public interface ICachable
{
    public bool IsExpired { get; }
}