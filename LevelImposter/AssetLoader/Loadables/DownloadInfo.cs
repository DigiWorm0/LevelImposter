using System;

namespace LevelImposter.AssetLoader;

public readonly record struct DownloadInfo(
    string downloadURL,
    string outputPath,
    Action<float>? onProgress = null,
    Action<DownloadResult>? onComplete = null,
    Action<string>? onError = null
) : IIdentifiable
{
    public string DownloadURL => downloadURL;
    public string OutputFilePath => outputPath;
    public Action<float>? OnProgress => onProgress;
    public Action<DownloadResult>? OnComplete => onComplete;
    public Action<string>? OnError => onError;
    public string ID => OutputFilePath;
}