namespace LevelImposter.Core.Models;

/// <summary>
///     A list of constants used within LevelImposter
/// </summary>
public static class LIConstants
{
    public const StringNames MAP_STRING_NAME = (StringNames)392001; // Placeholder StringNames for "Random Map"
    public const float PLAYER_POS = -5.0f; // Z value of the player
    public const int MAX_CONNECTION_TIMEOUT = 10; // Maximum time to wait for a client connection

    public const bool FREEPLAY_FLUSH_CACHE = true; // Whether to flush the cache when entering freeplay maps
    public const bool IS_DEVELOPMENT_BUILD = false; // Whether this is a development build
}