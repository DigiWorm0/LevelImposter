using System.IO;

namespace LevelImposter.Core;

/// <summary>
/// Represents a specific chunk (offset + length) of a file as a data store.
/// A stream opened from this store will only allow access to the specified chunk.
/// </summary>
public class FileChunkStore(string filePath, long offset, long length) : IDataStore
{
    public IMemoryBlock LoadToMemory()
    {
        using var stream = OpenStream();
        return stream.ToDataBlock();
    }

    public Stream OpenStream()
    {
        return new FileChunkStream(filePath, offset, length);
    }
}