using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class AudioLoader : AsyncQueue<AudioInfo, AudioResult>
{
    private AudioLoader()
    {
    }

    public static AudioLoader Instance { get; } = new();

    /// <summary>
    ///     Loads an AudioClip asynchronously from the given asset ID.
    /// </summary>
    /// <param name="assetID">Asset ID of the audio clip to load</param>
    /// <param name="onLoad">Callback invoked when the audio clip is loaded</param>
    /// <param name="isLobby">True to use AssetDB in lobby map. False otherwise</param>
    public static void LoadAsync(Guid? assetID, Action<AudioClip> onLoad, bool isLobby = false)
    {
        if (assetID == null)
            return;

        // Get Data Store from AssetDB
        var assetDB =
            isLobby ? GameConfiguration.CurrentLobbyMap?.mapAssetDB : GameConfiguration.CurrentMap?.mapAssetDB;
        var soundDataStore = assetDB?.Get(assetID);
        if (soundDataStore == null)
            return;

        // Create LoadableAudio
        var loadableAudio = new AudioInfo(assetID?.ToString() ?? "", soundDataStore);
        loadableAudio.Options.GCBehavior = GCBehavior.DisposeOnMapUnload; // TODO: Make configurable for lobbies

        // Enqueue Loadable
        Instance.AddToQueue(loadableAudio, loadedAudioClip => onLoad(loadedAudioClip.AudioClip));
    }

    protected override AudioResult Load(AudioInfo loadable)
    {
        var audioClip = WAVLoader.Load(loadable.DataStore, loadable.ID);
        return new AudioResult(audioClip);
    }
}