using System;
using System.IO;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ByteArray = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>;

namespace LevelImposter.Core;

/// <summary>
/// Represents a block of data in IL2CPP memory.
/// Must be disposed of after use to free resources.
/// </summary>
public class MemoryBlock
{
    /// <summary>
    /// The buffer size to use when reading from streams.
    /// This much memory will be allocated in managed memory during the read operation.
    /// </summary>
    private const int STREAM_BUFFER_SIZE = 1024 * 1024; // 1 MB
    // private static readonly byte[] StreamBuffer = new byte[STREAM_BUFFER_SIZE];
    
    public ByteArray Data { get; }

    public MemoryBlock(int length)
    {
        Data = new ByteArray(length);
    }
    private MemoryBlock(ByteArray data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        Data = data;
    }
    
    /// <summary>
    /// Shortcut to index into the memory block.
    /// </summary>
    /// <param name="index">The index to access.</param>
    public byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }
    
    /// <summary>
    /// The length of the memory block.
    /// </summary>
    public long Length => Data.Length;
    
    /// <summary>
    /// Pointer to the start of the actual array data in IL2CPP memory.
    /// </summary>
    public IntPtr BasePointer => IntPtr.Add(Data.Pointer, 4 * IntPtr.Size);

    /// <summary>
    /// Loads the entire stream into IL2CPP memory.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="length">The length of data to read from the stream. Negative values will result in stream.Length being used.</param>
    /// <returns>The memory block containing the data.</returns>
    public static unsafe MemoryBlock FromStream(Stream stream, long length = -1)
    {
        // Validate length
        if (length < 0)
            length = stream.Length;
        if (length > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(stream), "Stream is too large to fit in a MemoryBlock.");

        var chunkBuffer = Marshal.AllocHGlobal(STREAM_BUFFER_SIZE);
        var memoryBlock = new MemoryBlock((int)length);

        try
        {
            var totalRead = 0;

            while (totalRead < length)
            {
                var toRead = (int)Math.Min(STREAM_BUFFER_SIZE, length - totalRead);
                var span = new Span<byte>((void*)chunkBuffer, toRead);

                var bytesRead = stream.Read(span);
                if (bytesRead == 0)
                    break;

                var destPtr = IntPtr.Add(memoryBlock.BasePointer, totalRead);
                Buffer.MemoryCopy((void*)chunkBuffer, (void*)destPtr, memoryBlock.Length - totalRead, bytesRead);

                totalRead += bytesRead;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(chunkBuffer);
        }
        
        return memoryBlock;
    }

    /// <summary>
    /// Copies the data in this memory block to a managed byte array.
    /// </summary>
    /// <returns>>The managed byte array containing the data.</returns>
    public byte[] ToManagedArray()
    {
        var managedArray = new byte[Data.Length];
        Buffer.BlockCopy(Data, 0, managedArray, 0, Data.Length);
        return managedArray;
    }
    
    /// <summary>
    /// Creates a MemoryBlock from a managed byte array.
    /// Inefficient for large arrays since it copies data byte-by-byte.
    /// Try to use <see cref="FromStream"/> whenever possible.
    /// </summary>
    /// <param name="data">The managed byte array to copy data from.</param>
    /// <returns>>The memory block containing the data.</returns>
    public static MemoryBlock FromArray(byte[] data)
    {
        var memoryBlock = new MemoryBlock(data.Length);
        for (var i = 0; i < data.Length; i++)
            memoryBlock.Data[i] = data[i];
        return memoryBlock;
    }

    /// <summary>
    /// Creates a MemoryBlock from an IL2CPP byte array.
    /// </summary>
    /// <param name="data">The IL2CPP byte array to use.</param>
    /// <returns>>The memory block containing the data.</returns>
    public static MemoryBlock FromIl2CppArray(ByteArray data)
    {
        return new MemoryBlock(data);
    }
}