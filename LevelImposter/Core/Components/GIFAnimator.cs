using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Component to animate GIF data in-game
    /// </summary>
    public class GIFAnimator : MonoBehaviour
    {
        public GIFAnimator(IntPtr intPtr) : base(intPtr)
        {
        }

        private static Dictionary<long, GIFAnimator> _allAnimators = new();
        private static long _nextAnimatorID = 1;
        private static readonly List<string> AUTOPLAY_BLACKLIST = new()
        {
            "util-vent1",
            "util-vent2",
            "sab-doorv",
            "sab-doorh",
            "util-cam"
        };

        public Il2CppValueField<long> AnimatorID; // Unique ID maintained on instantiation
        public bool IsAnimating => _isAnimating;

        private bool _defaultLoopGIF = false;
        private bool _isAnimating = false;
        private int _frame = 0;
        private GIFFile? _gifData = null;
        private SpriteRenderer? _spriteRenderer;
        private Coroutine? _animationCoroutine = null;
        private long _animatorID => AnimatorID.Get();

        /// <summary>
        /// Initializes the component with GIF data
        /// </summary>
        /// <param name="element">Element that is initialized</param>
        /// <param name="sprites">Array of sprites representing each frame</param>
        /// <param name="frameTimes">Array of floats representing the times each frame is visible</param>
        [HideFromIl2Cpp]
        public void Init(LIElement element, GIFFile? gifData)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _gifData = gifData;
            _defaultLoopGIF = element.properties.loopGIF ?? true;

            bool shouldPreload = LIShipStatus.Instance?.CurrentMap?.properties?.preloadAllGIFs ?? false;
            bool isLowMemory = GCHandler.IsLowMemory();
            if (shouldPreload || isLowMemory)
                _gifData?.RenderAllFrames();

            if (_gifData?.Frames.Count == 1)
                _spriteRenderer.sprite = _gifData.GetFrameSprite(0); // Still image
            else if (AUTOPLAY_BLACKLIST.Contains(element.type))
                Stop(); // Don't autoplay
            else
                Play(); // Autoplay
        }

        /// <summary>
        /// Plays the GIF animation with default options
        /// </summary>
        public void Play()
        {
            Play(_defaultLoopGIF, false);
        }

        /// <summary>
        /// Plays the GIF animation with custom options
        /// </summary>
        /// <param name="repeat">True iff the GIF should loop</param>
        /// <param name="reverse">True iff the GIF should play in reverse</param>
        public void Play(bool repeat, bool reverse)
        {
            if (_gifData == null)
                LILogger.Warn($"{name} does not have any data");
            if (_spriteRenderer == null)
                LILogger.Warn($"{name} does not have a spriteRenderer");
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(CoAnimate(repeat, reverse).WrapToIl2Cpp());
        }

        /// <summary>
        /// Stops the GIF animation
        /// </summary>
        public void Stop(bool reversed = false)
        {
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
            _isAnimating = false;

            if (_spriteRenderer != null && _gifData != null)
            {
                _frame = reversed ? _gifData.Frames.Count - 1 : 0;
                _spriteRenderer.sprite = _gifData.GetFrameSprite(_frame);
                _spriteRenderer.enabled = true;
            }
        }

        /// <summary>
        /// Coroutine to run GIF animation
        /// </summary>
        /// <param name="repeat">TRUE if animation should loop</param>
        /// <param name="reverse">TRUE if animation should run in reverse</param>
        /// <returns>IEnumerator for Unity Coroutine</returns>
        [HideFromIl2Cpp]
        private IEnumerator CoAnimate(bool repeat, bool reverse)
        {
            if (_gifData == null || _spriteRenderer == null)
                yield break;
            _isAnimating = true;
            _spriteRenderer.enabled = true;
            while (_isAnimating)
            {
                // Wait for main thread
                while (!LagLimiter.ShouldContinue(60))
                    yield return null;

                // Render sprite
                _spriteRenderer.sprite = _gifData.GetFrameSprite(_frame);

                // Wait for next frame
                yield return new WaitForSeconds(_gifData.Frames[_frame].Delay);

                // Update frame index
                _frame = reverse ? _frame - 1 : _frame + 1;

                // Keep frame in bounds
                bool isOutOfBounds = _frame < 0 || _frame >= _gifData.Frames.Count;
                _frame = (_frame + _gifData.Frames.Count) % _gifData.Frames.Count;

                // Stop if out of bounds
                if (isOutOfBounds && !repeat)
                    Stop(!reverse);
            }
        }

        /// <summary>
        /// Fires when the animator is cloned
        /// </summary>
        /// <param name="original"></param>
        private void OnClone(GIFAnimator originalAnim)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _gifData = originalAnim._gifData;
            _defaultLoopGIF = originalAnim._defaultLoopGIF;
        }

        public void Awake()
        {
            // Check if object was cloned
            if (_animatorID != 0)
            {
                bool objectExists = _allAnimators.TryGetValue(_animatorID, out var originalObject);
                if (objectExists && originalObject != null)
                {
                    OnClone(originalObject);
                }
            }

            // Update Object ID
            AnimatorID.Set(_nextAnimatorID++);
            _allAnimators.Add(_animatorID, this);
        }
        public void OnDestroy()
        {
            _allAnimators.Remove(_animatorID);

            _gifData = null;
            _spriteRenderer = null;
            _animationCoroutine = null;
        }
    }
}
