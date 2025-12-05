using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace LevelImposter.Core;

public class ZIPEntryStream : Stream
{
    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => _zipArchiveEntry.Length; // Length is stored in the ZIP entry metadata

    private readonly ZipArchive _zipArchive;
    private readonly ZipArchiveEntry _zipArchiveEntry;
    private readonly Stream _baseStream;
    
    /// <summary>
    /// Represents a stream to a specific entry within a ZIP file.
    /// </summary>
    /// <param name="zipFilePath">The path to the ZIP file.</param>
    /// <param name="zipEntryName">The name of the entry within the ZIP file.</param>
    /// <exception cref="FileNotFoundException">Thrown if the specified ZIP entry is not found.</exception>
    public ZIPEntryStream(string zipFilePath, string zipEntryName)
    {
        // Open the ZIP archive
        _zipArchive = ZipFile.OpenRead(zipFilePath);
        
        // Get the specified entry
        var zipArchiveEntry = _zipArchive.GetEntry(zipEntryName);
        if (zipArchiveEntry == null)
            throw new FileNotFoundException($"ZIP entry '{zipEntryName}' not found in file '{zipFilePath}'");
        _zipArchiveEntry = zipArchiveEntry;
        
        // Open the entry stream
        _baseStream = _zipArchiveEntry.Open();
    }

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    public override void Flush()
    {
        _baseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset1, int count)
    {
        return _baseStream.Read(buffer, offset1, count);
    }

    public override long Seek(long seekOffset, SeekOrigin origin)
    {
        return _baseStream.Seek(seekOffset, origin);
    }

    public override void SetLength(long value)
    {
        _baseStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _baseStream.Write(buffer, offset, count);
    }

    public override string? ToString()
    {
        return _baseStream.ToString();
    }

    public override void Close()
    {
        _baseStream.Close();
        base.Close();
    }

    protected override void Dispose(bool disposing)
    {
        _baseStream.Dispose();
        _zipArchive.Dispose();
        base.Dispose(disposing);
    }
}