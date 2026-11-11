using System;
using System.IO;
using ByteArray = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>;

namespace LevelImposter.FileIO.DataBlock;

/// <summary>
///     Thin wrapper around Il2CppStructArray(byte).
///     Represents a block of data in IL2CPP memory.
/// </summary>
public class MemoryBlock
{
    /// <summary>
    ///     Makes a new MemoryBlock of the specified length.
    /// </summary>
    /// <param name="length">The length of the memory block in bytes.</param>
    public MemoryBlock(int length)
    {
        Data = new ByteArray(length);
        Length = length;
    }

    /// <summary>
    ///     Makes a new MemoryBlock wrapping an existing IL2CPP byte array.
    ///     Passing a managed byte array will automatically convert it to an IL2CPP array.
    /// </summary>
    /// <param name="data">The existing IL2CPP byte array.</param>
    public MemoryBlock(ByteArray data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        Data = data;
        Length = data.Length;
    }

    /// <summary>
    ///     Makes a new MemoryBlock wrapping an existing IL2CPP byte array.
    ///     Passing a managed byte array will automatically convert it to an IL2CPP array.
    ///     In addition, length is clamped to the provided value.
    /// </summary>
    /// <param name="data">The existing IL2CPP byte array.</param>
    /// <param name="length">The size to clamp the IL2CPP byte array.</param>
    public MemoryBlock(ByteArray data, int length)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        Data = data;
        Length = Math.Min(length, data.Length);
    }

    /// <summary>
    ///     The raw IL2CPP byte array
    /// </summary>
    public ByteArray Data { get; }

    /// <summary>
    ///     Shortcut to index into the memory block.
    /// </summary>
    /// <param name="index">The index to access.</param>
    public byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }

    /// <summary>
    ///     The length of the memory block.
    /// </summary>
    public int Length { get; }

    /// <summary>
    ///     Pointer to the start of the actual array data in IL2CPP memory.
    /// </summary>
    public IntPtr BasePointer => IntPtr.Add(Data.Pointer, 4 * IntPtr.Size);

    /// <summary>
    ///     Creates a span over the data in this memory block.
    ///     No data is copied or cloned, utilizes the existing IL2CPP memory.
    /// </summary>
    /// <returns>A span over the data in this memory block.</returns>
    public Span<byte> ToSpan()
    {
        return new Span<byte>(Data, 0, Length);
    }

    /// <summary>
    ///     Opens a MemoryStream that points to the data in IL2CPP memory.
    /// </summary>
    /// <returns>Stream that points to the data in IL2CPP memory.</returns>
    public unsafe Stream OpenStream()
    {
        return new UnmanagedMemoryStream((byte*)BasePointer, Length);
    }
}