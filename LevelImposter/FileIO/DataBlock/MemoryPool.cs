using System;

namespace LevelImposter.FileIO.DataBlock;

/// <summary>
///     Represents a reusable memory pool that can provide temporary memory blocks of varying sizes.
///     The provided memory block must be returned before it can be reused.
/// </summary>
/// <param name="initialCapacity">
///     The initial capacity of the memory pool in bytes.
///     If the requested size exceeds this capacity, the pool will automatically resize.
/// </param>
public class MemoryPool(int initialCapacity = 0)
{
    /// <summary>
    ///     Default shared instance of the memory pool for general use.
    /// </summary>
    public static readonly MemoryPool Shared = new();

    private MemoryBlock _internalMemory = new(initialCapacity);

    private bool _isInUse;

    /// <summary>
    ///     Uses the memory pool to get a temporary memory block of the specified size.
    /// </summary>
    /// <param name="size">The size of the temporary memory block to use.</param>
    /// <returns>A temporary memory block of the specified size.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the memory pool is already in use.
    ///     Call Release() before using again.
    /// </exception>
    public SharedMemoryBlock Rent(int size)
    {
        // Check for resource allocation
        if (_isInUse)
            throw new InvalidOperationException("Memory pool is already in use. Call Release() before using again.");
        _isInUse = true;

        // Resize internal memory
        if (size > _internalMemory.Length)
            ResizeTo(size);

        return new SharedMemoryBlock(this, size);
    }

    private void Release()
    {
        _isInUse = false;
    }

    private void ResizeTo(int size)
    {
        _internalMemory = new MemoryBlock(size);
    }

    public class SharedMemoryBlock : MemoryBlock, IDisposable
    {
        private readonly MemoryPool? _parentPool;

        internal SharedMemoryBlock(MemoryPool pool, int size) : base(pool._internalMemory.Data, size)
        {
            _parentPool = pool;
        }

        public SharedMemoryBlock(MemoryBlock block) : base(block.Data, block.Length)
        {
        }

        public void Dispose()
        {
            _parentPool?.Release();
        }
    }
}