using System;

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

    public byte[] LoadToManagedMemory()
    {
        return memoryBlock.ToManagedArray();
    }

    public byte[] Peek(int count)
    {
        var managedArray = new byte[count];
        Buffer.BlockCopy(memoryBlock.Data, 0, managedArray, 0, (int)Math.Min(count, memoryBlock.Length));
        return managedArray;
    }
}