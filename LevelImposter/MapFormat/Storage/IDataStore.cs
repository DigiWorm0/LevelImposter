using System.IO;

namespace LevelImposter.MapFormat.Storage;

/// <summary>
///     Represents a chunk of lazily-loadable byte data.
///     Used to avoid loading large data blocks into memory until needed.
/// </summary>
public interface IDataStore
{
    /// <summary>
    ///     Allows peeking at the first N bytes of the data without loading the entire block.
    /// </summary>
    /// <param name="count">Number of bytes to peek at from the start of the data.</param>
    /// <returns>A managed byte array containing the data.</returns>
    public byte[] Peek(int count);

    /// <summary>
    ///     Opens a stream to read the data.
    ///     The returned stream is owned by the caller and must be disposed.
    /// </summary>
    /// <returns>A stream over the data.</returns>
    public Stream OpenStream();
}
