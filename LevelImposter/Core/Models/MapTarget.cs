namespace LevelImposter.Core.Models;

/// <summary>
///     Represents the targeted area of <see cref="LIMap" />.
/// </summary>
public enum MapTarget
{
    Game = 0,
    Lobby = 1,

    // Mod-only attribute to indicate this runs on both the game and lobby maps
    Both = -1
}