using System.Buffers.Binary;
using System.IO;

namespace LevelImposter.Builders.Util;

public static class BinaryReaderExtensions
{
    /// <summary>
    ///     Reads an unsigned 32-bit integer from the current stream using big-endian encoding.
    /// </summary>
    /// <param name="reader">The BinaryReader instance to read from.</param>
    /// <returns>The unsigned 32-bit integer read from the stream.</returns>
    public static uint ReadUInt32_BigEndian(this BinaryReader reader)
    {
        var buffer = reader.ReadBytes(4);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }
}