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
    /// <param name="data">Optional preloaded data block</param>
    /// <returns>A fully-loaded GIFFile containing the image data</returns>
    public static LoadedGIFTexture Load(LoadableTexture loadable, MemoryBlock? data = null)
    {
        // Get whether to add to GC
        var addToGC = loadable.Options?.AddToGC ?? true;
        
        // Create new file
        var gifFile = new GIFFile(loadable.ID);
        if (addToGC)
            GCHandler.Register(gifFile);

        // Load data into managed memory
        var il2cppData = data ?? loadable.DataStore.LoadToMemory();
        var managedData = il2cppData.ToManagedArray();
        
        // Load the GIF file from the stream
        using var imgStream = new MemoryStream(managedData);
        gifFile.Load(imgStream, addToGC);
        
        // Return the GIF file
        return new LoadedGIFTexture(gifFile);
    }

    public class LoadedGIFTexture(GIFFile gifFile) : LoadedTexture(gifFile.GetFrameTexture(0))
    {
        public GIFFile GIFFile => gifFile;
    }
}