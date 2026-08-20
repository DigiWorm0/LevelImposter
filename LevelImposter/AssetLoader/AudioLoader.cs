using System;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.AssetLoader.Loaders;
using LevelImposter.AssetLoader.Queue;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.Utils;
using LevelImposter.Test;
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
        var loadable = GetLoadable(assetID, isLobby);
        if (loadable == null)
            return;

        // Enqueue Loadable
        Instance.AddToQueue(
            loadable.GetValueOrDefault(),
            loadedAudioClip => onLoad(loadedAudioClip.AudioClip));
    }

    private static AudioInfo? GetLoadable(Guid? assetID, bool isLobby = false)
    {
        if (assetID == null)
            return null;

        // Get Data Store from AssetDB
        var assetDB =
            isLobby ? GameConfiguration.CurrentLobbyMap?.MapAssetDB : GameConfiguration.CurrentMap?.MapAssetDB;
        var soundDataStore = assetDB?.Get(assetID);
        if (soundDataStore == null)
            return null;

        // Create LoadableAudio
        var loadableAudio = new AudioInfo(assetID.ToString() ?? "", soundDataStore);
        loadableAudio.Options.GCBehavior =
            isLobby
                ? GCBehavior.DisposeOnLobbyUnload
                : GCBehavior.DisposeOnMapUnload; // TODO: Make configurable for lobbies

        return loadableAudio;
    }

    /// <summary>
    ///     Loads an AudioClip synchronously from the given asset ID.
    /// </summary>
    /// <param name="assetID">Asset ID of the audio clip to load</param>
    /// <param name="isLobby">True to use AssetDB in lobby map. False otherwise</param>
    /// <returns>The corresponding AudioClip or null if not found</returns>
    public static AudioClip? LoadSync(Guid? assetID, bool isLobby = false)
    {
        var loadable = GetLoadable(assetID, isLobby);
        if (loadable == null)
            return null;

        return Instance.LoadImmediate(loadable.GetValueOrDefault()).AudioClip;
    }

    protected override AudioResult Load(AudioInfo loadable)
    {
        using var _ = Profiler.Measure("AudioLoader.Load", loadable.ID);

        var audioClip = WAVLoader.Load(loadable.DataStore, loadable.ID);
        return new AudioResult(audioClip);
    }
}