using System;
using System.Threading.Tasks;
using LevelImposter.Core.Utils;

namespace LevelImposter.AssetLoader.Queue;

public abstract class AssetLoader<TInput, TOutput>
    where TInput : IIdentifiable
    where TOutput : ICachable
{
    private readonly ItemCache<AssetLoad> _cache = new();
    public int QueueSize { get; private set; }

    /// <summary>
    ///     Short-hand to load asset then call a callback on the main Unity thread
    /// </summary>
    /// <param name="input">The input data to load the asset</param>
    /// <param name="onLoad">Callback when the asset is loaded</param>
    public void Load(TInput input, Action<TOutput> onLoad)
    {
        Load(input).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the exception
                LILogger.Warn($"Failed to load asset: {task.Exception}");
                return;
            }

            var result = task.Result;
            UnityThreadQueue.Run(() => onLoad(result));
        });
    }

    public async Task<TOutput> Load(TInput input)
    {
        // Check if the asset is already cached
        var cachedAsset = _cache.Get(input.ID);
        if (cachedAsset != null)
            return await cachedAsset.CompletionSource.Task;

        // If not cached, create a new task to load the asset
        var completionSource = new TaskCompletionSource<TOutput>(TaskCreationOptions.RunContinuationsAsynchronously);
        _cache.Add(input.ID, new AssetLoad(completionSource));

        // Load the asset asynchronously
        QueueSize++;

        try
        {
            var asset = await Task.Run(() => LoadAsset(input));
            completionSource.SetResult(asset);
        }
        catch (Exception ex)
        {
            completionSource.SetException(ex);
        }

        QueueSize--;

        return await completionSource.Task;
    }

    protected abstract Task<TOutput> LoadAsset(TInput input);

    private class AssetLoad(TaskCompletionSource<TOutput> completionSource) : ICachable
    {
        public TaskCompletionSource<TOutput> CompletionSource => completionSource;

        // public bool IsExpired => false;
        public bool IsExpired => CompletionSource.Task.IsCompleted &&
                                 CompletionSource.Task.Result.IsExpired;
    }
}