using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class AudioLoader : AsyncQueue<LoadableAudio, AudioClip>
{
    private AudioLoader()
    {
    }

    public static AudioLoader Instance { get; } = new();

    /// <summary>
    ///     Simplified shorthand to load an audioclip asynchronously.
    /// </summary>
    /// <param name="id">ID for cache</param>
    /// <param name="streamable">Streamable with raw image data</param>
    /// <param name="callback">Callback when the AudioClip is loaded</param>
    public static void LoadAsync(string id, IStreamable streamable, Action<AudioClip> callback)
    {
        Instance.AddToQueue(
            new LoadableAudio(id, streamable),
            callback
        );
    }

    protected override AudioClip Load(LoadableAudio loadable)
    {
        // Open the stream
        using var stream = loadable.Streamable.OpenStream();

        // Load the sprite
        var loadedFile = WAVLoader.Load(stream, loadable.ID);

        // Return the loaded sprite
        return loadedFile;
    }
}