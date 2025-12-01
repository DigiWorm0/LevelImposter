using System;
using System.Collections;
using System.Collections.Generic;
using LevelImposter.Core;
using Reactor.Utilities;

namespace LevelImposter.AssetLoader;

/// <summary>
///     A queue that asynchronously loads items in the background using a coroutine.
/// </summary>
/// <typeparam name="TInput">Type used for input values (must be ICachable)</typeparam>
/// <typeparam name="TOutput">Type used for output values (must be ICachable)</typeparam>
public abstract class AsyncQueue<TInput, TOutput> where TInput : ICachable
{
    private IEnumerator? _consumeQueueCoroutine;

    public int QueueSize => Queue.Count;
    public int CacheSize => Cache.Count;
    protected Queue<QueuedItem> Queue { get; } = new();
    protected ItemCache<TOutput> Cache { get; } = new();

    /// <summary>
    ///     Adds an item to the queue.
    /// </summary>
    /// <param name="inputData">Input data needed to load the item</param>
    /// <param name="onLoad">Called when the item is loaded in</param>
    public void AddToQueue(TInput inputData, Action<TOutput> onLoad)
    {
        // Add the item to the queue
        Queue.Enqueue(new QueuedItem(inputData, onLoad));

        // Start consuming the queue if it's not already running
        _consumeQueueCoroutine ??= Coroutines.Start(CoConsumeQueue());
    }

    /// <summary>
    ///     Clears the queue and cache.
    /// </summary>
    public void Clear()
    {
        Queue.Clear();
        Cache.Clear();
    }

    /// <summary>
    ///     Called when an item is loaded.
    ///     <para>
    ///         Should be implemented to define how to load an item from input data.
    ///         Do not call this method directly;
    ///         Instead, use <see cref="AddToQueue"/> or <see cref="LoadImmediate"/>.
    ///     </para>
    /// </summary>
    /// <param name="inputData">Input data used to load item</param>
    /// <returns>Output data of the item</returns>
    protected abstract TOutput Load(TInput inputData);

    /// <summary>
    ///     Unity coroutine for consuming the queue.
    /// </summary>
    private IEnumerator CoConsumeQueue()
    {
        // Repeat until the queue is empty
        while (Queue.Count > 0)
        {
            // Wait for the next available frame
            yield return null;

            // Continuously load items until the lag limit is reached
            while (LagLimiter.ShouldContinue(20) && Queue.Count > 0)
                ConsumeQueueOnce();

        }

        // Clear the coroutine
        _consumeQueueCoroutine = null;
    }

    /// <summary>
    /// Loads a single item from the queue.
    /// </summary>
    private void ConsumeQueueOnce()
    {
        try
        {
            // Get the next item in the queue
            var queuedItem = Queue.Dequeue();
            
            // Load the item
            var output = LoadImmediate(queuedItem.InputData);

            // Call the onLoad callback
            queuedItem.OnLoad(output);
        }
        catch (Exception e)
        {
            LILogger.Error(e);
        }
    }

    /// <summary>
    /// Loads an item immediately, checking the cache first.
    /// </summary>
    /// <param name="inputData">Loadable item data</param>
    /// <returns>Loaded item</returns>
    protected TOutput LoadImmediate(TInput inputData)
    {
        // Check if item is in cache
        var output = Cache.Get(inputData.ID);
        if (output != null)
            return output;
        
        // Load the item
        output = Load(inputData);

        // Add the item to the cache
        Cache.Add(inputData.ID, output);

        return output;
    }

    /// <summary>
    ///     Represents an item in the queue.
    /// </summary>
    /// <param name="inputData">Data used for input</param>
    /// <param name="onLoad">Callback when the data is loaded in</param>
    protected readonly struct QueuedItem(TInput inputData, Action<TOutput> onLoad)
    {
        public string ID => InputData.ID;
        public TInput InputData { get; } = inputData;
        public Action<TOutput> OnLoad { get; } = onLoad;
    }
}