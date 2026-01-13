using System.IO;

namespace LevelImposter.Core;

/// <summary>
/// Represents an entire file as a data store.
/// See <see cref="FileChunkStore"/> for chunked file access.
/// </summary>
/// <param name="filePath">The path to the file.</param>
public class FileStore(string filePath) : IDataStore
{
    public MemoryBlock LoadToMemory()
    {
        using var stream = File.OpenRead(filePath);
        return MemoryBlock.FromStream(stream);
    }
}