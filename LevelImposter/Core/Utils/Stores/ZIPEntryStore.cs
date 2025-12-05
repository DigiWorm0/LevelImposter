using System.IO;

namespace LevelImposter.Core;

public class ZIPEntryStore(string zipFilePath, string zipEntryName) : IDataStore
{
    public IMemoryBlock LoadToMemory()
    {
        using var stream = OpenStream();
        return stream.ToDataBlock();
    }

    public Stream OpenStream()
    {
        return new ZIPEntryStream(zipFilePath, zipEntryName);
    }
}