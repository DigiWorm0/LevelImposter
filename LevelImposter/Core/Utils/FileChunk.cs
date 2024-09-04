using System.IO;

namespace LevelImposter.Core;

public class FileChunk
{
    private readonly string _filePath;
    private readonly long _length = -1;
    private readonly long _offset = -1;

    public FileChunk(string filePath, long offset, long length)
    {
        _filePath = filePath;
        _offset = offset;
        _length = length;
    }

    /// <summary>
    ///     Opens a stream to the file chunk.
    /// </summary>
    /// <returns>A stream to the cooresponding file chunk</returns>
    public FileChunkStream OpenStream()
    {
        var fileStream = File.OpenRead(_filePath);
        return new FileChunkStream(fileStream, _offset, _length);
    }
}