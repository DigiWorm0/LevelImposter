namespace LevelImposter.Core
{
    enum Layer
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Water = 4,
        UI,
        Players = 8,
        Ship,
        Shadow,
        Objects, // <-- Hidden by util-display
        ShortObjects,
        IlluminatedBlocking,
        Ghost,
        UICollider,
        DrawShadows
    }
}
