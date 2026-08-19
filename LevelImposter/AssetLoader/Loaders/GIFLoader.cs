using System.IO;
using System.Threading.Tasks;
using LevelImposter.AssetLoader.FileContainers;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.Test;

namespace LevelImposter.AssetLoader.Loaders;

public static class GIFLoader
{
    public static async Task<GifTextureResult> Load(TextureInfo loadable)
    {
        using var _ = Profiler.Measure("GIFLoader.Load", loadable.ID);

        // Create new file
        var gifFile = new GIFFile(loadable.ID);

        // Load data into managed memory
        var imgData = loadable.DataStore.LoadToManagedMemory();

        // Load the GIF file from the stream
        using var imgStream = new MemoryStream(imgData);
        gifFile.Load(imgStream, loadable.Options.GCBehavior);

        // Load the 1st frame
        await UnityThreadQueue.Run(() => gifFile.RenderFrame(0));

        // Return the GIF file
        return new GifTextureResult(gifFile);
    }

    public class GifTextureResult(GIFFile gifFile) : TextureResult(gifFile.DefaultTexture)
    {
        public GIFFile GIFFile => gifFile;
    }
}