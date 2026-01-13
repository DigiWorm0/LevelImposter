using System;
using ByteArray = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>;

namespace LevelImposter.Core;

/// <summary>
/// Thin wrapper around Il2CppStructArray<byte>.
/// Represents a block of data in IL2CPP memory.
/// </summary>
public class MemoryBlock
{
    /// <summary>
    /// The raw IL2CPP byte array
    /// </summary>
    public ByteArray Data { get; }
    
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
    /// Makes a new MemoryBlock of the specified length.
    /// </summary>
    /// <param name="length">The length of the memory block in bytes.</param>
    public MemoryBlock(int length)
    {
        Data = new ByteArray(length);
    }

    /// <summary>
    /// Makes a new MemoryBlock wrapping an existing IL2CPP byte array.
    /// Passing a managed byte array will automatically convert it to an IL2CPP array.
    /// </summary>
    /// <param name="data">The existing IL2CPP byte array.</param>
    public MemoryBlock(ByteArray data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        Data = data;
    }

    /// <summary>
    /// Copies the data in this memory block to a managed byte array.
    /// </summary>
    /// <returns>The managed byte array containing the data.</returns>
    public byte[] ToManagedArray()
    {
        var managedArray = new byte[Data.Length];
        Buffer.BlockCopy(Data, 0, managedArray, 0, Data.Length);
        return managedArray;
    }
}