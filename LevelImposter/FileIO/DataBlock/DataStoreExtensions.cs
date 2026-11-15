using LevelImposter.FileIO.Streams;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.FileIO.DataBlock;

/// <summary>
///     Runtime-only conveniences for moving <see cref="IDataStore" /> data into IL2CPP memory.
///     These are intentionally not part of <see cref="IDataStore" /> so the map-format core stays Unity-free.
/// </summary>
public static class DataStoreExtensions
{
    /// <summary>
    ///     Loads the entire data store into IL2CPP memory as a <see cref="MemoryBlock" />.
    /// </summary>
    /// <param name="dataStore">The data store to read.</param>
    /// <returns>The IL2CPP memory block containing the data.</returns>
    public static MemoryBlock LoadToMemory(this IDataStore dataStore)
    {
        using var stream = dataStore.OpenStream();
        return stream.ToIl2CppArray();
    }

    /// <summary>
    ///     Loads the entire data store into a shared IL2CPP memory space as a <see cref="MemoryBlock" />.
    ///     Once the data is no longer needed, it must be disposed to free memory.
    /// </summary>
    /// <param name="dataStore">The data store to read.</param>
    /// <returns>The shared IL2CPP memory block containing the data.</returns>
    public static MemoryPool.SharedMemoryBlock LoadToSharedMemory(this IDataStore dataStore)
    {
        using var stream = dataStore.OpenStream();
        var sharedMemory = MemoryPool.Shared.Rent((int)stream.Length);
        stream.CopyTo(sharedMemory);
        return sharedMemory;
    }
}