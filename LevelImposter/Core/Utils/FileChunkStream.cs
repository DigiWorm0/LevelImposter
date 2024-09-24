using System;
using System.IO;
using System.Text;

namespace LevelImposter.Core;

public class FileChunkStream : Stream
{
    private readonly FileStream _fileStream;
    private readonly long _length = -1;
    private readonly long _offset = -1;
    private long _position;

    public FileChunkStream(FileStream fileStream, long offset, long length)
    {
        _fileStream = fileStream;
        _offset = offset;
        _length = length;
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override void Flush()
    {
        // Nothing to flush
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _fileStream.Seek(_offset + _position, SeekOrigin.Begin);
        var read = _fileStream.Read(buffer, offset, count);
        _position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = offset;
                break;
            case SeekOrigin.Current:
                _position += offset;
                break;
            case SeekOrigin.End:
                _position = _length - offset;
                break;
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
        var buffer = new byte[_length];
        Read(buffer, 0, (int)_length);
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