using System;
using System.Buffers;
using System.IO;

namespace LevelImposter.Core;

public interface IDataStore
{
    /// <summary>
    /// Loads the entire data into memory as an <see cref="IMemoryBlock"/>
    /// This is required for formats such as PNG
    /// that require all data to be present in memory
    /// (since they're loaded by Unity APIs).
    /// 
    /// To avoid double-buffering, this is provided separately from <see cref="OpenStream"/>.
    /// Once the data is no longer needed, it must be disposed to free memory.
    /// </summary>
    /// <returns>The byte array containing the data.</returns>
    public IMemoryBlock LoadToMemory();
    
    /// <summary>
    /// Opens a stream to read the data.
    /// </summary>
    /// <returns>>A Stream to read the data.</returns>
    public Stream OpenStream();
}