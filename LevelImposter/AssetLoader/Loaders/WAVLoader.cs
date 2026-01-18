using System.IO;
using LevelImposter.Core;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public static class WAVLoader
{
    /// <summary>
    /// Loads a WAV from a sound data object.
    /// </summary>
    /// <param name="soundData">Sound Data to load</param>
    /// <returns>Sound data in the form of a Unity AudioClip</returns>
    public static AudioClip? Load(LISound? soundData)
    {
        // Get Sound Data
        if (soundData == null)
            return null;

        // Get data from Map Asset DB
        var soundDBElem = GameConfiguration.CurrentMap?.mapAssetDB?.Get(soundData.dataID);
        if (soundDBElem == null)
            return null;

        // Load from data store
        return Load(soundDBElem, soundData.id.ToString());
    }

    /// <summary>
    /// Loads a WAV from a data store.
    /// </summary>
    /// <param name="dataStore">Data store containing the WAV data</param>
    /// <param name="name">Name of the resulting object</param>
    /// <param name="gcBehavior">Garbage collection behavior for the loaded AudioClip</param>
    /// <returns>Sound data in the form of a Unity AudioClip</returns>
    public static AudioClip Load(
        IDataStore dataStore,
        string name,
        GCBehavior? gcBehavior = null)
    {
        // Load data into managed memory
        var wavData = dataStore.LoadToManagedMemory();
        
        // Load the WAV from the stream
        using var stream = new MemoryStream(wavData);
        return Load(stream, name, gcBehavior);
    }
    
    /// <summary>
    /// Loads a WAV from a raw stream.
    /// </summary>
    /// <param name="wavStream">Raw WAV file stream</param>
    /// <param name="name">Name of the resulting object</param>
    /// <param name="gcBehavior">Garbage collection behavior for the loaded AudioClip</param>
    /// <returns>Sound data in the form of a Unity AudioClip</returns>
    public static AudioClip Load(
        Stream wavStream,
        string name,
        GCBehavior? gcBehavior = null)
    {
        // Create a new WAV file
        var wavFile = new WAVFile(name);
        GCHandler.Register(wavFile, gcBehavior);

        // Load the WAV file from the stream
        wavFile.Load(wavStream);

        // Return the WAV file
        return wavFile.GetClip();
    }
}