using System;
using System.IO;
using System.IO.Compression;
using LevelImposter.MapFormat.Storage;

namespace LevelImposter.CoreTests.Storage;

public class ZIPEntryStoreTests
{
    private static readonly byte[] EntryData = [0x21, 0x32, 0x43, 0x54, 0x65];
    private const string EntryName = "assets/sprite.bin";

    [Fact]
    public void Peek_ShouldReturnRequestedPrefix()
    {
        var path = CreateArchive();
        try
        {
            var store = new ZIPEntryStore(path, EntryName);

            store.Peek(3).ShouldBe([0x21, 0x32, 0x43]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Peek_ShouldPadWithZerosWhenRequestExceedsEntryLength()
    {
        var path = CreateArchive();
        try
        {
            var store = new ZIPEntryStore(path, EntryName);

            store.Peek(7).ShouldBe([0x21, 0x32, 0x43, 0x54, 0x65, 0x00, 0x00]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void OpenStream_ShouldExposeEntireEntry()
    {
        var path = CreateArchive();
        try
        {
            var store = new ZIPEntryStore(path, EntryName);
            using var stream = store.OpenStream();

            stream.Length.ShouldBe(EntryData.Length);
            stream.ToManagedArray().ShouldBe(EntryData);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void OpenStream_ShouldThrowWhenEntryDoesNotExist()
    {
        var path = CreateArchive();
        try
        {
            var store = new ZIPEntryStore(path, "missing.bin");

            Should.Throw<FileNotFoundException>(() => store.OpenStream());

            // The failed open must release the archive handle.
            var movedPath = path + ".moved";
            File.Move(path, movedPath);
            File.Delete(movedPath);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static string CreateArchive()
    {
        var path = Path.Combine(Path.GetTempPath(), $"li-zip-store-{Guid.NewGuid():N}.zip");
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        using var entryStream = archive.CreateEntry(EntryName).Open();
        entryStream.Write(EntryData);
        return path;
    }
}
