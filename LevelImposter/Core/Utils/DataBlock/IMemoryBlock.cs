using System;

namespace LevelImposter.Core;

/// <summary>
/// Represents a block of data in memory.
/// Must be disposed of after use to free resources.
/// </summary>
public interface IMemoryBlock : IDisposable
{
    public byte[] Get();
}