namespace LevelImposter.Core;

public interface IBoolValue
{
    public const int MAX_DEPTH = 255; // Prevent infinite loops

    public bool GetValue(int depth = 0);
}