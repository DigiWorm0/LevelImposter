using System.IO;
using FileMode = Il2CppSystem.IO.FileMode;
using Il2CppFile = Il2CppSystem.IO.File;

namespace LevelImposter.Core;

/// <summary>
/// Represents a specific chunk (offset + length) of a file as a data store.
/// A stream opened from this store will only allow access to the specified chunk.
/// </summary>
public class FileChunkStore(string filePath, long offset, long length) : IDataStore
{
    public MemoryBlock LoadToMemory()
    {
        using var stream = new FileChunkStream(filePath, offset, length);
        return MemoryBlock.FromStream(stream);
    }
}