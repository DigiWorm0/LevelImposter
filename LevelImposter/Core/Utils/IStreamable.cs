using System.IO;

namespace LevelImposter.Core;

public interface IStreamable
{
    public Stream OpenStream();
}