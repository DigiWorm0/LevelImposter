using System;
using LevelImposter.Core;

namespace LevelImposter.AssetLoader;

public class DownloadResult(FileStore store) : ICachable
{
    private readonly DateTime _downloadTime = DateTime.Now;
    private readonly TimeSpan _expireDuration = TimeSpan.FromMinutes(5);

    public FileStore Store => store;
    public bool IsExpired => DateTime.Now - _downloadTime > _expireDuration;
}