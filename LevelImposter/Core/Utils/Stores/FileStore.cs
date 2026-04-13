using System;
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
        stream.Read(managedArray, 0, count);
        return managedArray;
    }

    /// <summary>
    /// Opens a stream to read the entire file.
    /// </summary>
    /// <returns>The file stream.</returns>
    public Stream OpenStream()
    {
        return File.OpenRead(filePath);
    }
}