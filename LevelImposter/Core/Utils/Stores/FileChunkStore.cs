using System;

namespace LevelImposter.Core;

/// <summary>
/// Represents a specific chunk (offset + length) of a file as a data store.
/// A stream opened from this store will only allow access to the specified chunk.
/// </summary>
public class FileChunkStore(string filePath, long offset, long length) : IDataStore
{
    public MemoryBlock LoadToMemory()
    {
        using var stream = OpenStream();
        return stream.ToIl2CppArray();
    }

    public byte[] LoadToManagedMemory()
    {
        using var stream = OpenStream();
        return stream.ToManagedArray();
    }

    public byte[] Peek(int count)
    {
        using var stream = OpenStream();
        var managedArray = new byte[count];
        stream.Read(managedArray, 0, (int)Math.Min(count, length));
        return managedArray;
    }

    /// <summary>
    /// Opens a stream to read from the file chunk.
    /// </summary>
    /// <returns>The file chunk stream.</returns>
    public FileChunkStream OpenStream()
    {
        return new FileChunkStream(filePath, offset, length);
    }
}