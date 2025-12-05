using System;
using System.IO;
using System.Text;

namespace LevelImposter.Core;

/// <summary>
///     Represents a stream to a specific length and offset within a file.
/// </summary>
/// <param name="filePath">Path to the file</param>
/// <param name="offset">Offset of the stream in bytes</param>
/// <param name="length">Length of the stream in bytes</param>
public class FileChunkStream(string filePath, long offset, long length) : Stream
{
    private readonly FileStream _fileStream = File.OpenRead(filePath);
    private long _position;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => length;

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override void Flush()
    {
        // Nothing to flush
    }

    public override int Read(byte[] buffer, int offset1, int count)
    {
        _fileStream.Seek(offset + _position, SeekOrigin.Begin);
        var read = _fileStream.Read(buffer, offset1, count);
        _position += read;
        return read;
    }

    public override long Seek(long seekOffset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = seekOffset;
                break;
            case SeekOrigin.Current:
                _position += seekOffset;
                break;
            case SeekOrigin.End:
                _position = length - seekOffset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }

        return _position;
    }

    public override void SetLength(long value)
    {
        // Length is read-only
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        // Write is not supported
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        // Read to buffer
        var buffer = new byte[length];
        var bytesRead = Read(buffer, 0, (int)length);

        // Check if the entire file chunk was read
        if (bytesRead != length)
            throw new IOException("Failed to read entire file chunk");

        // Convert buffer to string
        return Encoding.UTF8.GetString(buffer);
    }

    public override void Close()
    {
        _fileStream.Close();
        base.Close();
    }

    protected override void Dispose(bool disposing)
    {
        _fileStream.Dispose();
        base.Dispose(disposing);
    }
}