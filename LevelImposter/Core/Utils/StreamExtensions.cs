using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LevelImposter.Core;

/// <summary>
/// A set of extension methods for <see cref="Stream"/>.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// The buffer size to use when reading from streams.
    /// This much memory will be allocated in managed memory during the read operation.
    /// </summary>
    private const int STREAM_BUFFER_SIZE = 1024 * 1024; // 1 MB
    private static readonly IntPtr ChunkBuffer = Marshal.AllocHGlobal(STREAM_BUFFER_SIZE);

    /// <summary>
    ///   Reads a managed stream into a managed byte array.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>A managed byte array containing the data.</returns>
    public static byte[] ToManagedArray(this Stream stream)
    {
        // Ensure the stream length fits in an int
        long length = stream.Length;
        if (length > int.MaxValue)
            throw new InvalidOperationException("Stream too large");

        // Read the entire stream into buffer, one chunk at a time
        byte[] buffer = new byte[length];
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = stream.Read(buffer, offset, buffer.Length - offset);
            if (read == 0)
                break; // End of stream

            offset += read;
        }

        return buffer;
    }

    /// <summary>
    ///   Reads a managed stream into an IL2CPP MemoryBlock.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="length">The length of data to read from the stream. Negative values will result in stream.Length being used.</param>
    /// <returns>An IL2CPP MemoryBlock containing the data.</returns>
    public unsafe static MemoryBlock ToIl2CppArray(this Stream stream, long length = -1)
    {
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
}