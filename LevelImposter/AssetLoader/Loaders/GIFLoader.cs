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
    /// <param name="loadable">Loadable sprite object</param>
    /// <returns>A fully-loaded GIFFile containing the image data</returns>
    public static Texture2D Load(
        Stream imgStream,
        LoadableTexture loadable)
    {
        // Create new file
        var gifFile = new GIFFile(loadable.ID);
        GCHandler.Register(gifFile);

        // Load the GIF file from the stream
        gifFile.Load(imgStream);

        // Return the GIF file
        // TODO: Stow gifFile to use later for animated textures
        return gifFile.GetFrameTexture(0);
    }
}