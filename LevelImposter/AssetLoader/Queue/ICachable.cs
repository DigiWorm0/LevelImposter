namespace LevelImposter.AssetLoader.Queue;

public interface ICachable
{
    public bool IsExpired { get; }
}