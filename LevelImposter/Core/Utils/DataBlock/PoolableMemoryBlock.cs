using System;
using System.Buffers;

namespace LevelImposter.Core;

/// <summary>
/// Creates a memory block by renting a byte array from the shared array pool.
/// </summary>
/// <param name="size">The size of the memory block to rent.</param>
internal class PoolableMemoryBlock(int size) : IMemoryBlock
{
    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;
    private readonly byte[] _data = Pool.Rent(size);

    /// <summary>
    /// Returns the rented memory back to the pool.
    /// </summary>
    public void Dispose()
    {
        Pool.Return(_data);
    }

    public byte[] Get()
    {
        return _data;
    }
}