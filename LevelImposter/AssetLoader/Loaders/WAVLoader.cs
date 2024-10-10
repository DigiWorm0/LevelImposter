using System.IO;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class WAVLoader
{
    /// <summary>
    ///     Loads a WAV from a sound data object.
    /// </summary>
    /// <param name="soundData">Sound Data to load</param>
    /// <returns>Sound data in the form of a Unity AudioClip</returns>
    public static AudioClip? Load(LISound? soundData)
    {
        // Get Sound Data
        if (soundData == null)
            return null;

        // Get Asset DB
        var mapAssetDB = LIShipStatus.GetInstanceOrNull()?.CurrentMap?.mapAssetDB;

        // Get Sound Stream
        var soundDBElem = mapAssetDB?.Get(soundData.dataID);
        if (soundDBElem == null)
            return null;

        // Get Sound Data
        var stream = soundDBElem.OpenStream();
        var audioClip = Load(stream, soundData.id.ToString());
        stream.Close();

        return audioClip;
    }

    /// <summary>
    ///     Loads a WAV from a raw stream.
    /// </summary>
    /// <param name="stream">Stream to loads from</param>
    /// <param name="name">Name of the resulting object</param>
    /// <returns>Sound data in the form of a Unity AudioClip</returns>
    public static AudioClip Load(Stream stream, string name)
    {
        // Create a new WAV file
        var wavFile = new WAVFile(name);
        GCHandler.Register(wavFile);

        // Load the WAV file from the stream
        wavFile.Load(stream);

        // Return the WAV file
        return wavFile.GetClip();
    }
}