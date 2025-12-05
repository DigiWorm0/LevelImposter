using System.IO;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public static class GIFLoader
{
    /// <summary>
    ///     Loads a GIF image from a stream.
    /// </summary>
    /// <param name="loadable">Loadable sprite object</param>
    /// <returns>A fully-loaded GIFFile containing the image data</returns>
    public static LoadedGIFTexture Load(LoadableTexture loadable)
    {
        // Get whether to add to GC
        var addToGC = loadable.Options?.AddToGC ?? true;
        
        // Create new file
        var gifFile = new GIFFile(loadable.ID);
        if (addToGC)
            GCHandler.Register(gifFile);

        // Load the GIF file from the stream
        using var imgStream = loadable.DataStore.OpenStream();
        gifFile.Load(imgStream, addToGC);
        
        // Return the GIF file
        return new LoadedGIFTexture(gifFile);
    }

    public class LoadedGIFTexture(GIFFile gifFile) : LoadedTexture(gifFile.GetFrameTexture(0))
    {
        public GIFFile GIFFile => gifFile;
    }
}