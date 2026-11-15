using System;
using System.IO;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.CoreTests.Storage;

public class FileChunkStreamTests
{
    [Fact]
    public void Read_ShouldNotReadPastChunkBoundary()
    {
        var path = CreateFile(64);
        try
        {
            using var stream = new FileChunkStream(path, 10, 16);
            var buffer = new byte[64];

            var read = stream.Read(buffer, 0, buffer.Length);

            read.ShouldBe(16);
            buffer[..16].ShouldBe(BytesInRange(10, 16));
            stream.Read(buffer, 0, buffer.Length).ShouldBe(0);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Seek_ShouldRespectChunkBounds()
    {
        var path = CreateFile(64);
        try
        {
            using var stream = new FileChunkStream(path, 10, 16);
            var buffer = new byte[8];

            stream.Seek(0, SeekOrigin.End).ShouldBe(16);
            stream.Seek(-4, SeekOrigin.End).ShouldBe(12);
            stream.Read(buffer, 0, buffer.Length).ShouldBe(4);
            buffer[..4].ShouldBe(BytesInRange(22, 4));
            stream.Position.ShouldBe(16);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Length_ShouldBeChunkLength()
    {
        var path = CreateFile(64);
        try
        {
            using var stream = new FileChunkStream(path, 4, 32);

            stream.Length.ShouldBe(32);
            stream.Position.ShouldBe(0);
            stream.CanRead.ShouldBeTrue();
            stream.CanSeek.ShouldBeTrue();
            stream.CanWrite.ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] BytesInRange(int start, int count)
    {
        var bytes = new byte[count];
        for (var i = 0; i < count; i++)
            bytes[i] = (byte)(start + i);
        return bytes;
    }

    private static string CreateFile(int length)
    {
        var path = Path.Combine(Path.GetTempPath(), $"li-test-{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(path, BytesInRange(0, length));
        return path;
    }
}
