using System;
using System.IO;

namespace LevelImposter.MapFormat.Storage;

/// <summary>
///     Represents a managed byte array as a data store.
///     This is the portable alternative to <c>MemoryStore</c>, which wraps IL2CPP memory.
/// </summary>
/// <param name="data">The managed data to store.</param>
public class MemoryStore(byte[] data) : IDataStore
{
    public byte[] Peek(int count)
    {
        var peeked = new byte[count];
        Buffer.BlockCopy(data, 0, peeked, 0, Math.Min(count, data.Length));
        return peeked;
    }

    public Stream OpenStream()
    {
        return new MemoryStream(data, false);
    }
}