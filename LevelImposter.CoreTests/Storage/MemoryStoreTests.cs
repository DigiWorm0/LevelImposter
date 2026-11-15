using System.IO;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.CoreTests.Storage;

public class MemoryStoreTests
{
    private static readonly byte[] Data = [0x10, 0x20, 0x30, 0x40];

    [Fact]
    public void Peek_ShouldReturnRequestedPrefix()
    {
        var store = new MemoryStore(Data);

        store.Peek(2).ShouldBe([0x10, 0x20]);
    }

    [Fact]
    public void Peek_ShouldPadWithZerosWhenRequestExceedsDataLength()
    {
        var store = new MemoryStore(Data);

        store.Peek(6).ShouldBe([0x10, 0x20, 0x30, 0x40, 0x00, 0x00]);
    }

    [Fact]
    public void OpenStream_ShouldExposeEntireDataAsReadOnlyStream()
    {
        var store = new MemoryStore(Data);

        using var stream = store.OpenStream();

        stream.CanRead.ShouldBeTrue();
        stream.CanWrite.ShouldBeFalse();
        stream.ToManagedArray().ShouldBe(Data);
    }
}
