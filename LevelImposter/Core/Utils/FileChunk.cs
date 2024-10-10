using System.IO;

namespace LevelImposter.Core;

/// <summary>
///     Represents a specific chunk of a file that can be streamed.
/// </summary>
public class FileChunk : IStreamable
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
    /// <returns>A FileChunkStream to the cooresponding file chunk</returns>
    public Stream OpenStream()
    {
        var fileStream = File.OpenRead(_filePath);
        return new FileChunkStream(fileStream, _offset, _length);
    }
}