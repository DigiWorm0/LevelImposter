namespace LevelImposter.Core;

internal enum Layer
{
    /// <summary>
    ///     Only visible in light
    /// </summary>
    Default,
    TransparentFX,
    IgnoreRaycast,
    Water = 4,

    /// <summary>
    ///     Full-brightness and always visible
    /// </summary>
    UI,
    Players = 8,
    Ship,

    /// <summary>
    ///     Only visible in shadow, blocks light
    /// </summary>
    Shadow,

    /// <summary>
    ///     Automatically hidden by <c>util-display</c> objects
    /// </summary>
    Objects,

    ShortObjects,
    IlluminatedBlocking,
    Ghost,
    UICollider,
    DrawShadows,
    KeyMapper,
    MusicTriggers,
    Notifications
}