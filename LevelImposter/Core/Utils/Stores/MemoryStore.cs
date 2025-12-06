using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Core;

/// <summary>
/// Stores a block of data in memory.
/// </summary>
/// <param name="rawData">The raw data to store.</param>
public class MemoryStore(byte[] rawData) : IDataStore
{
    public IMemoryBlock LoadToMemory()
    {
        // We can reuse the existing data block since the data is already in memory.
        return new ExistingMemoryBlock(rawData);
    }
    
    public Stream OpenStream()
    {
        return new MemoryStream(rawData);
    }
}