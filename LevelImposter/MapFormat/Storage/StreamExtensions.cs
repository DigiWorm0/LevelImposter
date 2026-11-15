using System.IO;

namespace LevelImposter.MapFormat.Storage;

/// <summary>
///     Portable stream read helpers used by the map-format core.
///     Kept separate from <c>LevelImposter.FileIO.Streams.StreamExtensions</c>,
///     which depends on IL2CPP memory types.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    ///     Reads exactly <paramref name="count" /> bytes into <paramref name="buffer" />,
    ///     throwing if the stream ends early.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="buffer">The buffer to read into.</param>
    /// <param name="count">The number of bytes to read.</param>
    /// <exception cref="EndOfStreamException">Thrown if the stream ends before <paramref name="count" /> bytes are read.</exception>
    public static void ReadExactly(this Stream stream, byte[] buffer, int count)
    {
        var offset = 0;
        while (offset < count)
        {
            var read = stream.Read(buffer, offset, count - offset);
            if (read <= 0)
                throw new EndOfStreamException($"Unexpected end of stream while reading {count} bytes");
            offset += read;
        }
    }

    /// <summary>
    ///     Reads exactly <paramref name="count" /> bytes into a new managed array,
    ///     throwing if the stream ends early.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>A new array containing the read bytes.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the stream ends before <paramref name="count" /> bytes are read.</exception>
    public static byte[] ReadBytes(this Stream stream, int count)
    {
        var buffer = new byte[count];
        stream.ReadExactly(buffer, count);
        return buffer;
    }

    /// <summary>
    ///     Reads a managed stream into a managed byte array.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>A managed byte array containing the data.</returns>
    public static byte[] ToManagedArray(this Stream stream)
    {
        var buffer = new byte[stream.Length];
        stream.ReadExactly(buffer, (int)stream.Length);
        return buffer;
    }
}