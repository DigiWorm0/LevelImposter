using System;
using System.Buffers;
using System.IO;

namespace LevelImposter.Core;

/// <summary>
/// Lazy data store interface.
/// </summary>
public interface IDataStore
{
    /// <summary>
    /// Loads the entire data into memory as an <see cref="MemoryBlock"/>.
    /// Allows large chunks of data to be lazily loaded when needed.
    /// Once the data is no longer needed, it must be disposed to free memory.
    /// </summary>
    /// <returns>The byte array containing the data.</returns>
    public MemoryBlock LoadToMemory();
}