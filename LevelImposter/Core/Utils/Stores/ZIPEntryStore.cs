using System.IO;

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
        using var stream = new ZIPEntryStream(zipFilePath, zipEntryName);
        return MemoryBlock.FromStream(stream);
    }
}