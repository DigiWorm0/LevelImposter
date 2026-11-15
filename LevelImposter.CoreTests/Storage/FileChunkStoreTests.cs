using System;
using System.IO;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.CoreTests.Storage;

public class FileChunkStoreTests
{
    private static readonly byte[] FileData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

    [Fact]
    public void Peek_ShouldReadFromChunkOffset()
    {
        var path = CreateFile();
        try
        {
            var store = new FileChunkStore(path, 4, 5);

            store.Peek(3).ShouldBe([4, 5, 6]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Peek_ShouldNotReadPastChunkAndShouldPadWithZeros()
    {
        var path = CreateFile();
        try
        {
            var store = new FileChunkStore(path, 4, 3);

            store.Peek(5).ShouldBe([4, 5, 6, 0, 0]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void OpenStream_ShouldExposeOnlyTheRequestedChunk()
    {
        var path = CreateFile();
        try
        {
            var store = new FileChunkStore(path, 4, 5);
            using var stream = store.OpenStream();

            stream.Length.ShouldBe(5);
            stream.ToManagedArray().ShouldBe([4, 5, 6, 7, 8]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"li-chunk-store-{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(path, FileData);
        return path;
    }
}
