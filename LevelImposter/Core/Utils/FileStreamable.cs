using System.IO;

namespace LevelImposter.Core;

public class FileStreamable(string _filePath) : IStreamable
{
    public Stream OpenStream()
    {
        return File.OpenRead(_filePath);
    }
}