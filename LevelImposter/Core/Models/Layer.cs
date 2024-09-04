namespace LevelImposter.Core;

internal enum Layer
{
    Default,
    TransparentFX,
    IgnoreRaycast,
    Water = 4,
    UI,
    Players = 8,
    Ship,
    Shadow,

    /// <summary>
    ///     WARNING: Automatically hidden by <c>util-display</c> objects
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