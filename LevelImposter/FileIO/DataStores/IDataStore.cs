using LevelImposter.FileIO.DataBlock;

namespace LevelImposter.FileIO.DataStores;

/// <summary>
///     Represents a chunk of lazily-loadable byte data.
///     Used to avoid loading large data blocks into memory until needed.
/// </summary>
public interface IDataStore
{
    /// <summary>
    ///     Loads the entire data into IL2CPP memory as an <see cref="MemoryBlock" />.
    ///     Allows large chunks of data to be lazily loaded when needed.
    /// </summary>
    /// <returns>The IL2CPP MemoryBlock containing the data.</returns>
    public MemoryBlock LoadToMemory();

    /// <summary>
    ///     Loads the entire data into a shared IL2CPP memory space as an <see cref="MemoryBlock" />.
    ///     Allows large chunks of data to be lazily loaded when needed.
    ///     Once the data is no longer needed, it must be disposed to free memory.
    /// </summary>
    /// <returns>The IL2CPP MemoryBlock containing the data.</returns>
    public MemoryPool.SharedMemoryBlock LoadToSharedMemory();

    /// <summary>
    ///     Allows peeking at the first N bytes of the data without loading the entire block.
    /// </summary>
    /// <param name="count">Number of bytes to peek at from the start of the data.</param>
    /// <returns>A managed byte array containing the data.</returns>
    public byte[] Peek(int count);
}