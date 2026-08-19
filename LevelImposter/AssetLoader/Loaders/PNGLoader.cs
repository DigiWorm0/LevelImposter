using System;
using System.IO;
using System.Threading.Tasks;
using LevelImposter.AssetLoader.FileContainers;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.Test;

namespace LevelImposter.AssetLoader.Loaders;

public static class PNGLoader
{
    public static async Task<PngTextureResult> Load(TextureInfo loadable)
    {
        using var _ = Profiler.Measure("GIFLoader.Load", loadable.ID);

        // Create new file
        var pngFile = new PNGFile(loadable.ID);

        // Load data into managed memory
        var imgData = loadable.DataStore.LoadToManagedMemory();

        // Load the GIF file from the stream
        using var imgStream = new MemoryStream(imgData);
        pngFile.Load(imgStream);

        // Load the 1st frame
        await UnityThreadQueue.Run(() => pngFile.Render());

        // Return the GIF file
        return new PngTextureResult(pngFile);
    }

    public class PngTextureResult(PNGFile pngFile)
        : TextureResult(pngFile.OutputTexture ?? throw new Exception("PNG has no texture"))
    {
        public PNGFile PNGFile => pngFile;
    }
}