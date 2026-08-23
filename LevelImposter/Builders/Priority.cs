namespace LevelImposter.Builders;

/// <summary>
///     Map Builder priority levels
/// </summary>
public static class Priority
{
    /// SpriteRenderers and Colliders are added at this level
    public const int FIRST = 1000;

    public const int VERY_HIGH = 500;
    public const int HIGH = 100;
    public const int DEFAULT = 0;
    public const int LOW = -100;
    public const int VERY_LOW = -500;
    public const int LAST = -1000;
}