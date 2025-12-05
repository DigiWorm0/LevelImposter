using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Core;

public class MemoryStreamable(Il2CppArrayBase<byte> rawData) : IStreamable
{
    public Stream OpenStream()
    {
        return new MemoryStream(rawData);
    }
}