using System;
using System.IO;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.CoreTests.Storage;

public class FileStoreTests
{
    private static readonly byte[] Data = [0x11, 0x22, 0x33, 0x44, 0x55];

    [Fact]
    public void Peek_ShouldReturnRequestedPrefix()
    {
        var path = CreateFile(Data);
        try
        {
            var store = new FileStore(path);

            store.Peek(3).ShouldBe([0x11, 0x22, 0x33]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Peek_ShouldPadWithZerosWhenRequestExceedsFileLength()
    {
        var path = CreateFile(Data);
        try
        {
            var store = new FileStore(path);

            store.Peek(7).ShouldBe([0x11, 0x22, 0x33, 0x44, 0x55, 0x00, 0x00]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void OpenStream_ShouldExposeEntireFile()
    {
        var path = CreateFile(Data);
        try
        {
            var store = new FileStore(path);
            using var stream = store.OpenStream();

            stream.Length.ShouldBe(Data.Length);
            stream.ToManagedArray().ShouldBe(Data);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateFile(byte[] data)
    {
        var path = Path.Combine(Path.GetTempPath(), $"li-store-{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(path, data);
        return path;
    }
}
