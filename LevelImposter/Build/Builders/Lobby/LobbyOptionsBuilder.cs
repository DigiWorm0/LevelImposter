using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;

namespace LevelImposter.Build.Builders.Lobby;

internal static class LobbyOptionsBuilder
{
    private const string AMBIENT_SOUND_NAME = "lobbyAmbientNoise";
    private const string AMBIENT_MUSIC_NAME = "lobbyMusic";
    private const string SPAWN_IN_NOISE_NAME = "lobbySpawnInNoise";

    [ElementBuilder(
        Target = MapTarget.Lobby,
        ElementTypes = ["util-lobbyoptions"]
    )]
    public static void Build(LobbyBehaviour lobbyBehaviour, LIElement element)
    {
        if (element.type != "util-lobbyoptions")
            return;

        // Ambient Sound
        var ambientSound = element.properties.sounds.FindSound(AMBIENT_SOUND_NAME);
        if (ambientSound != null)
            lobbyBehaviour.DropShipSound = WAVLoader.Load(ambientSound, true) ?? lobbyBehaviour.DropShipSound;

        // Ambient Music
        var ambientMusic = element.properties.sounds.FindSound(AMBIENT_MUSIC_NAME);
        if (ambientMusic != null)
            lobbyBehaviour.MapTheme = WAVLoader.Load(ambientMusic, true) ?? lobbyBehaviour.MapTheme;

        // Spawn-In Noise
        var spawnInNoise = element.properties.sounds.FindSound(SPAWN_IN_NOISE_NAME);
        if (spawnInNoise != null)
            lobbyBehaviour.SpawnSound = WAVLoader.Load(spawnInNoise, true) ?? lobbyBehaviour.SpawnSound;
    }
}