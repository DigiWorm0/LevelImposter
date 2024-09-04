using System;
using System.Diagnostics;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Limits the amount of time a coroutine can divert the main thread
/// </summary>
public class LagLimiter(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private Stopwatch? _frameTimer; // Basic FPS counter
    private bool _hasContinuedThisFrame; // Continue at least once per frame

    public static LagLimiter? Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        _frameTimer?.Restart();
        _hasContinuedThisFrame = false;
    }

    public void OnEnable()
    {
        _frameTimer = new Stopwatch();
        _frameTimer?.Start();
    }

    public void OnDisable()
    {
        _frameTimer?.Stop();
    }

    /// <summary>
    ///     Returns true if the coroutine should continue
    /// </summary>
    /// <param name="minFPS">Minimum FPS to maintain</param>
    /// <returns><c>true</c> if the coroutine should continue, <c>false</c> otherwise</returns>
    private bool CheckShouldContinue(float minFPS)
    {
        float elapsedMilliseconds = _frameTimer?.ElapsedMilliseconds ?? 0;
        var currentFPS = 1000.0f / elapsedMilliseconds;
        var shouldContinue = currentFPS > minFPS || !_hasContinuedThisFrame;
        _hasContinuedThisFrame = true;
        return shouldContinue;
    }

    public static bool ShouldContinue(float minFPS)
    {
        return Instance?.CheckShouldContinue(minFPS) ?? true;
    }
}