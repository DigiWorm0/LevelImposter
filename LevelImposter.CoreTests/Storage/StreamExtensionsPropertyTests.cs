using System;
using System.IO;
using FsCheck;
using FsCheck.Xunit;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.CoreTests.Storage;

public class StreamExtensionsPropertyTests
{
    [Property]
    public bool ReadBytes_ShouldReassembleData_WhenReadsAreShort(byte[] data, int requestedSize)
    {
        var maxReadSize = (requestedSize & 0x7) + 1;
        using var stream = new ShortReadStream(data, maxReadSize);

        var read = stream.ReadBytes(data.Length);

        return read.AsSpan().SequenceEqual(data);
    }

    [Property]
    public bool ToManagedArray_ShouldReadEntireStream_WhenReadsAreShort(byte[] data, int requestedSize)
    {
        var maxReadSize = (requestedSize & 0x7) + 1;
        using var stream = new ShortReadStream(data, maxReadSize);

        var read = stream.ToManagedArray();

        return read.AsSpan().SequenceEqual(data);
    }

    [Property]
    public bool ReadExactly_ShouldThrow_WhenStreamEndsEarly(byte[] data)
    {
        if (data.Length == 0)
            return true;

        using var stream = new ShortReadStream(data, 1);

        try
        {
            stream.ReadExactly(new byte[data.Length + 1], data.Length + 1);
            return false;
        }
        catch (EndOfStreamException)
        {
            return true;
        }
    }

    /// <summary>
    ///     Emulates network/file streams, which are free to return fewer bytes than
    ///     requested even when more data is available.
    /// </summary>
    private sealed class ShortReadStream(byte[] data, int maxReadSize) : MemoryStream(data, false)
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            return base.Read(buffer, offset, Math.Min(count, maxReadSize));
        }
    }
}
