using System.IO;

namespace LevelImposter.Core;

/// <summary>
/// Represents an entire file as a data store.
/// See <see cref="FileChunkStore"/> for chunked file access.
/// </summary>
/// <param name="filePath">The path to the file.</param>
public class FileStore(string filePath) : IDataStore
{
    public IMemoryBlock LoadToMemory()
    {
        using var stream = OpenStream();
        return stream.ToDataBlock();
    }

    public Stream OpenStream()
    {
        return File.OpenRead(filePath);
    }
}