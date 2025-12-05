using System;
using System.Buffers;

namespace LevelImposter.Core;

/// <summary>
/// A wrapper for an existing byte array as a memory block.
/// </summary>
/// <param name="data">The existing byte array.</param>
internal class ExistingMemoryBlock(byte[] data) : IMemoryBlock
{
    public void Dispose() {}

    public byte[] Get()
    {
        return data;
    }
}