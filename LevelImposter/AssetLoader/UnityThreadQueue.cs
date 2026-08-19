using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using LevelImposter.Core.Components;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.AssetLoader;

/// <summary>
///     Background component that allows for queuing actions to be performed on the main Unity thread.
/// </summary>
public class UnityThreadQueue(IntPtr ptr) : MonoBehaviour(ptr)
{
    private static int _mainThreadId;
    public static bool IsInMainThread => _mainThreadId == Environment.CurrentManagedThreadId;

    private static ConcurrentQueue<Action> Queue { get; } = new();
    private static int MinFPS => GameState.IsInMainMenu ? 60 : 5;

    public void Awake()
    {
        _mainThreadId = Environment.CurrentManagedThreadId;
    }

    public void Update()
    {
        // Continuously load items until the lag limit is reached
        while (LagLimiter.ShouldContinue(MinFPS)
               && !Queue.IsEmpty
               && Queue.TryDequeue(out var action))
            action();
    }

    /// <summary>
    ///     Asserts that the code is running in the main thread
    /// </summary>
    /// <param name="location">The location of the code that is asserting</param>
    /// <exception cref="InvalidOperationException">Thrown if the code is not running in the main thread</exception>
    public static void AssertMainThread(string location)
    {
        if (!IsInMainThread)
            throw new InvalidOperationException(
                $"Unity accessed from wrong thread at {location}. " +
                $"Thread={Thread.CurrentThread.ManagedThreadId}, " +
                $"MainThread={_mainThreadId}");
    }

    /// <summary>
    ///     Queues an action to perform on the main Unity thread.
    /// </summary>
    /// <param name="action">The action to perform on the main Unity thread.</param>
    /// <returns>A task that completes after the action has been performed</returns>
    public static Task Run(Action action)
    {
        var completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Queue.Enqueue(() =>
        {
            try
            {
                action();
                completion.SetResult(true);
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });

        return completion.Task;
    }

    /// <summary>
    ///     Queues an action to perform on the main Unity thread.
    /// </summary>
    /// <param name="action">The action to perform on the main Unity thread.</param>
    /// <returns>A task that completes after the action has been performed</returns>
    public static Task<T> Run<T>(Func<T> action)
    {
        var taskCompletionSource = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Queue.Enqueue(() =>
        {
            try
            {
                var result = action();
                taskCompletionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }
}