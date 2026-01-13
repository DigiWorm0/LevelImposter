using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Core;

/// <summary>
/// Wrapper around an existing <see cref="MemoryBlock"/> to provide it as an <see cref="IDataStore"/>.
/// Since the data is already in memory at construction time, it simply returns the existing block on load.
/// </summary>
/// <param name="memoryBlock">The existing memory block.</param>
public class MemoryStore(MemoryBlock memoryBlock) : IDataStore
{
    public MemoryBlock LoadToMemory()
    {
        return memoryBlock;
    }
}