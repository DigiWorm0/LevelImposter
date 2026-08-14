using System.IO;
using LevelImposter.AssetLoader.FileContainers;
using LevelImposter.AssetLoader.Loadables;

namespace LevelImposter.AssetLoader.Loaders;

public static class GIFLoader
{
    /// <summary>
    ///     Loads a GIF image from a stream.
    /// </summary>
    /// <param name="loadable">Loadable sprite object</param>
    /// <returns>A fully-loaded GIFFile containing the image data</returns>
    public static GifTextureResult Load(TextureInfo loadable)
    {
        // Create new file
        var gifFile = new GIFFile(loadable.ID);

        // Load data into managed memory
        var imgData = loadable.DataStore.LoadToManagedMemory();

        // Load the GIF file from the stream
        using var imgStream = new MemoryStream(imgData);
        gifFile.Load(imgStream, loadable.Options.GCBehavior);

        // Return the GIF file
        return new GifTextureResult(gifFile);
    }

    public class GifTextureResult(GIFFile gifFile) : TextureResult(gifFile.DefaultTexture)
    {
        public GIFFile GIFFile => gifFile;
    }
}