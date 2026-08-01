using System;

namespace LevelImposter.Core;

/// <summary>
/// Represents a data store of a specific entry within a ZIP archive.
/// Used for ZIP-compressed LIM files.
/// </summary>
/// <param name="zipFilePath">Full file path to the ZIP file.</param>
/// <param name="zipEntryName">The name of the entry within the ZIP file.</param>
public class ZIPEntryStore(string zipFilePath, string zipEntryName) : IDataStore
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
        stream.Read(managedArray, 0, (int)Math.Min(count, stream.Length));
        return managedArray;
    }

    /// <summary>
    /// Opens a stream to read the ZIP entry.
    /// </summary>
    /// <returns>The ZIP entry stream.</returns>
    public ZIPEntryStream OpenStream()
    {
        return new ZIPEntryStream(zipFilePath, zipEntryName);
    }
}