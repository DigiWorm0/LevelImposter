using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Components;
using UnityEngine;

namespace LevelImposter.Builders.Lobby;

public class LobbyOptionsBuilder : IElemBuilder
{
    private const string AMBIENT_SOUND_NAME = "lobbyAmbientNoise";
    private const string AMBIENT_MUSIC_NAME = "lobbyMusic";
    private const string SPAWN_IN_NOISE_NAME = "lobbySpawnInNoise";

    public void OnBuild(LIElement elem, GameObject gameObject)
    {
        if (elem.type != "util-lobbyoptions")
            return;

        var lobby = LILobbyBehaviour.GetLobbyBehaviour();

        // Ambient Sound
        var ambientSound = elem.properties.sounds.FindSound(AMBIENT_SOUND_NAME);
        if (ambientSound != null)
            lobby.DropShipSound = WAVLoader.Load(ambientSound, true) ?? lobby.DropShipSound;

        // Ambient Music
        var ambientMusic = elem.properties.sounds.FindSound(AMBIENT_MUSIC_NAME);
        if (ambientMusic != null)
            lobby.MapTheme = WAVLoader.Load(ambientMusic, true) ?? lobby.MapTheme;

        // Spawn-In Noise
        var spawnInNoise = elem.properties.sounds.FindSound(SPAWN_IN_NOISE_NAME);
        if (spawnInNoise != null)
            lobby.SpawnSound = WAVLoader.Load(spawnInNoise, true) ?? lobby.SpawnSound;
    }
}