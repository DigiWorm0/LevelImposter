using System;
using System.IO;

namespace LevelImposter.MapFormat.Storage;

/// <summary>
///     Represents a specific chunk (offset + length) of a file as a data store.
///     A stream opened from this store will only allow access to the specified chunk.
/// </summary>
public class FileChunkStore(string filePath, long offset, long length) : IDataStore
{
    public byte[] Peek(int count)
    {
        using var stream = OpenStream();
        var managedArray = new byte[count];
        stream.Read(managedArray, 0, (int)Math.Min(count, length));
        return managedArray;
    }

    /// <summary>
    ///     Opens a stream to read from the file chunk.
    /// </summary>
    /// <returns>The file chunk stream.</returns>
    public Stream OpenStream()
    {
        return new FileChunkStream(filePath, offset, length);
    }
}
