using System.IO;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class GIFLoader
{
    /// <summary>
    ///     Loads a GIF image from a stream.
    /// </summary>
    /// <param name="imgStream">Image stream to load from</param>
    /// <param name="options">Options to apply</param>
    /// <returns>A fully-loaded GIFFile containing the image data</returns>
    public static LoadedGIF Load(
        Stream imgStream,
        LoadableSprite loadable)
    {
        // Create new file
        var gifFile = new GIFFile(loadable.ID);
        GCHandler.Register(gifFile);

        // Append Options
        var options = loadable.Options;
        gifFile.SetPivot(options.Pivot);
        // TODO: Allow pixel art in GIFs

        // Load the GIF file from the stream
        gifFile.Load(imgStream);

        // Return the GIF file
        return new LoadedGIF(gifFile.GetFrameSprite(0), gifFile);
    }

    public class LoadedGIF(Sprite _sprite, GIFFile _gifFile) : LoadedSprite(_sprite)
    {
        public GIFFile GIFFile => _gifFile;
    }
}