using System;
using System.Diagnostics;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Limits the amount of time a coroutine can divert the main thread
    /// </summary>
    public class LagLimiter : MonoBehaviour
    {
        public LagLimiter(IntPtr intPtr) : base(intPtr)
        {
        }

        public static LagLimiter? Instance { get; private set; }

        private Stopwatch? _frameTimer = null; // Basic FPS counter
        private bool _hasContinuedThisFrame = false; // Continue at least once per frame

        /// <summary>
        /// Returns true if the coroutine should continue
        /// </summary>
        /// <param name="minFPS">Minimum FPS to maintain</param>
        /// <returns><c>true</c> if the coroutine should continue, <c>false</c> otherwise</returns>
        private bool CheckShouldContinue(float minFPS)
        {
            float elapsedMilliseconds = _frameTimer?.ElapsedMilliseconds ?? 0;
            float currentFPS = 1000.0f / elapsedMilliseconds;
            bool shouldContinue = currentFPS > minFPS || !_hasContinuedThisFrame;
            _hasContinuedThisFrame = true;
            return shouldContinue;
        }
        public static bool ShouldContinue(float minFPS) => Instance?.CheckShouldContinue(minFPS) ?? true;

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
        public void OnEnable()
        {
            _frameTimer = new();
            _frameTimer?.Start();
        }
        public void Update()
        {
            _frameTimer?.Restart();
            _hasContinuedThisFrame = false;
        }
        public void OnDisable()
        {
            _frameTimer?.Stop();
        }
    }
}
