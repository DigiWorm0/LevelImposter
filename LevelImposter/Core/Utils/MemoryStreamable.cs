using System.IO;

namespace LevelImposter.Core;

public class MemoryStreamable(byte[] _rawData) : IStreamable
{
    public Stream OpenStream()
    {
        return new MemoryStream(_rawData);
    }
}