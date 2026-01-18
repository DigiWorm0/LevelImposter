using System;
using LevelImposter.Core;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class AudioLoader : AsyncQueue<LoadableAudio, LoadedAudioClip>
{
    private AudioLoader()
    {
    }

    public static AudioLoader Instance { get; } = new();

    /// <summary>
    /// Loads an AudioClip asynchronously from the given asset ID.
    /// </summary>
    /// <param name="assetID">Asset ID of the audio clip to load</param>
    /// <param name="onLoad">Callback invoked when the audio clip is loaded</param>
    public static void LoadAsync(Guid? assetID, Action<AudioClip> onLoad)
    {
        if (assetID == null)
            return;
        
        // Get Data Store from AssetDB
        var soundDataStore = GameConfiguration.CurrentMap?.mapAssetDB?.Get(assetID);
        if (soundDataStore == null)
            return;

        // Create LoadableAudio
        var loadableAudio = new LoadableAudio(assetID?.ToString() ?? "", soundDataStore);
        loadableAudio.Options.GCBehavior = GCBehavior.DisposeOnMapUnload; // TODO: Make configurable for lobbies
        
        // Enqueue Loadable
        Instance.AddToQueue(loadableAudio, loadedAudioClip => onLoad(loadedAudioClip.AudioClip));
    }
    
    protected override LoadedAudioClip Load(LoadableAudio loadable)
    {
        var audioClip = WAVLoader.Load(loadable.DataStore, loadable.ID);
        return new LoadedAudioClip(audioClip);
    }
}