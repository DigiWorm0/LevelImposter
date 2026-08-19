using System;
using System.IO;
using System.Runtime.InteropServices;
using LevelImposter.AssetLoader;
using LevelImposter.FileIO.DataBlock;

namespace LevelImposter.FileIO.Streams;

/// <summary>
///     A set of extension methods for <see cref="Stream" />.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    ///     The buffer size to use when reading from streams.
    ///     This much memory will be allocated in managed memory during the read operation.
    /// </summary>
    private const int STREAM_BUFFER_SIZE = 1024 * 1024; // 1 MB

    private static readonly IntPtr ChunkBuffer = Marshal.AllocHGlobal(STREAM_BUFFER_SIZE);

    /// <summary>
    ///     Reads a managed stream into a managed byte array.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>A managed byte array containing the data.</returns>
    public static byte[] ToManagedArray(this Stream stream)
    {
        // Ensure the stream length fits in an int
        var length = stream.Length;
        if (length > int.MaxValue)
            throw new InvalidOperationException("Stream too large");

        // Read the entire stream into buffer, one chunk at a time
        var buffer = new byte[length];
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = stream.Read(buffer, offset, buffer.Length - offset);
            if (read == 0)
                break; // End of stream

            offset += read;
        }

        return buffer;
    }

    /// <summary>
    ///     Reads a managed stream into an IL2CPP MemoryBlock.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="length">
    ///     The length of data to read from the stream. Negative values will result in stream.Length being
    ///     used.
    /// </param>
    /// <returns>An IL2CPP MemoryBlock containing the data.</returns>
    public static unsafe MemoryBlock ToIl2CppArray(this Stream stream, long length = -1)
    {
        UnityThreadQueue.AssertMainThread("StreamExtensions.ToIl2CppArray");

        // Validate length
        if (length < 0)
            length = stream.Length;
        if (length > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(stream), "Stream is too large to fit in a MemoryBlock.");

        // Allocate IL2CPP memory
        var memoryBlock = new MemoryBlock((int)length);

        // Read stream one chunk at a time
        var totalRead = 0;
        while (totalRead < length)
        {
            // Calculate how many bytes to read
            var toRead = (int)Math.Min(STREAM_BUFFER_SIZE, length - totalRead);

            // Read into ChunkBuffer
            var span = new Span<byte>((void*)ChunkBuffer, toRead);
            var bytesRead = stream.Read(span);
            if (bytesRead == 0)
                break;

            // Use Buffer.MemoryCopy to copy memory from Managed to IL2CPP
            var destPtr = IntPtr.Add(memoryBlock.BasePointer, totalRead);
            Buffer.MemoryCopy((void*)ChunkBuffer, (void*)destPtr, memoryBlock.Length - totalRead, bytesRead);

            // Increment Read Head
            totalRead += bytesRead;
        }

        return memoryBlock;
    }

    /// <summary>
    ///     Polyfill for Stream.ReadExactly, which is not available in .NET Standard 2.1.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="buffer">The buffer to write to</param>
    /// <param name="offset">The offset in the buffer to start writing to</param>
    /// <param name="count">The number of bytes to read</param>
    /// <exception cref="EndOfStreamException">Thrown if the stream ends before reading the requested number of bytes</exception>
    public static void ReadExactly(
        this Stream stream,
        byte[] buffer,
        int offset,
        int count)
    {
        var totalRead = 0;
        while (totalRead < count)
        {
            var read = stream.Read(buffer, offset + totalRead, count - totalRead);
            if (read == 0)
                throw new EndOfStreamException(
                    $"End of stream reached. Did not read {count} bytes (read {totalRead} bytes instead).");
            totalRead += read;
        }
    }
}