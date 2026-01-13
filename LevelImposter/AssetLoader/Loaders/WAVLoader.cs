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
        var soundDBElem = MapLoader.CurrentMap?.mapAssetDB?.Get(soundData.dataID);
        if (soundDBElem == null)
            return null;
        
        // Load data into IL2CPP memory
        var il2cppData = soundDBElem.LoadToMemory();
        var managedData = il2cppData.ToManagedArray();

        // Get Sound Data from stream
        using var stream = new MemoryStream(managedData);
        return Load(stream, soundData.id.ToString());
    }

    /// <summary>
    /// Loads a WAV from a data store.
    /// </summary>
    /// <param name="dataStore">Data store containing the WAV data</param>
    /// <param name="name">Name of the resulting object</param>
    /// <returns>Sound data in the form of a Unity AudioClip</returns>
    public static AudioClip Load(IDataStore dataStore, string name)
    {
        // Load data into managed memory
        var il2cppData = dataStore.LoadToMemory();
        var managedData = il2cppData.ToManagedArray();
        
        // Load the WAV from the stream
        using var stream = new MemoryStream(managedData);
        return Load(stream, name);
    }
    
    /// <summary>
    /// Loads a WAV from a raw stream.
    /// </summary>
    /// <param name="wavStream">Raw WAV file stream</param>
    /// <param name="name">Name of the resulting object</param>
    /// <returns>Sound data in the form of a Unity AudioClip</returns>
    public static AudioClip Load(Stream wavStream, string name)
    {
        // Create a new WAV file
        var wavFile = new WAVFile(name);
        GCHandler.Register(wavFile);

        // Load the WAV file from the stream
        wavFile.Load(wavStream);

        // Return the WAV file
        return wavFile.GetClip();
    }
}