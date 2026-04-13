using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Base component to animate LI elements in-game.
///     Implemented by specific animator types (e.g. GIFAnimator, SpriteAnimator)
/// </summary>
public class LIAnimatorBase(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static readonly Dictionary<long, LIAnimatorBase> _allAnimators = new();
    private static long _nextAnimatorID = 1;

    private bool _loopByDefault;
    private int _frameIndex;
    private Coroutine? _animationCoroutine;
    private SpriteRenderer? _spriteRenderer;

    private long _animatorID => AnimatorID.Get();
    private int _frameCount => GetFrameCount();
    
    // This unique ID is maintained on instantiation
    public Il2CppValueField<long> AnimatorID;

    public bool IsAnimating { get; private set; }
    

    public void Awake()
    {
        // Check if object was cloned
        if (_animatorID != 0)
        {
            var objectExists = _allAnimators.TryGetValue(_animatorID, out var originalObject);
            if (objectExists && originalObject != null) OnClone(originalObject);
        }

        // Update Object ID
        AnimatorID.Set(_nextAnimatorID++);
        _allAnimators.Add(_animatorID, this);
    }

    public void OnDestroy()
    {
        _allAnimators.Remove(_animatorID);

        _spriteRenderer = null;
        _animationCoroutine = null;
    }

    /// <summary>
    ///     Initializes the component with LIElement data
    /// </summary>
    /// <param name="element">Element that is initialized</param>
    [HideFromIl2Cpp]
    protected void Init(LIElement element)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _loopByDefault = element.properties.loopGIF ?? true;

        // AutoPlay
        if (!IsReady())
            return;
        if (_frameCount == 1)
            _spriteRenderer.sprite = TryGetFrameSprite(0);
        else 
            Play();
    }

    /// <summary>
    ///     Plays the animation with default options
    /// </summary>
    public void Play()
    {
        Play(_loopByDefault, false);
    }

    /// <summary>
    ///     Plays the animation with custom options
    /// </summary>
    /// <param name="repeat">True iff the animation should loop</param>
    /// <param name="reverse">True iff the animation should play in reverse</param>
    public void Play(bool repeat, bool reverse)
    {
        if (_spriteRenderer == null)
            LILogger.Warn($"{name} does not have a spriteRenderer");
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(CoAnimate(repeat, reverse).WrapToIl2Cpp());
    }

    /**
     * Plays a specific type of animation, if supported by the animator
     * @param type The type of animation to play
     */
    public virtual void PlayType(string type)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Stops the animation
    /// </summary>
    public void Stop(bool reversed = false)
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        IsAnimating = false;

        if (_spriteRenderer == null)
            return;
        if (!IsReady())
            return;
        
        _frameIndex = reversed ? _frameCount - 1 : 0;
        _spriteRenderer.sprite = TryGetFrameSprite(_frameIndex);
        _spriteRenderer.enabled = true;
    }

    /// <summary>
    ///     Coroutine to run animation
    /// </summary>
    /// <param name="repeat">TRUE if animation should loop</param>
    /// <param name="reverse">TRUE if animation should run in reverse</param>
    /// <returns>IEnumerator for Unity Coroutine</returns>
    [HideFromIl2Cpp]
    private IEnumerator CoAnimate(bool repeat, bool reverse)
    {
        if (_spriteRenderer == null)
            yield break;
        
        // Flag Start
        IsAnimating = true;
        _spriteRenderer.enabled = true;

        // Reset frame
        if (reverse && _frameIndex == 0)
            _frameIndex = _frameCount - 1;
        else if (!reverse && _frameIndex == _frameCount - 1)
            _frameIndex = 0;

        // Loop
        while (IsAnimating)
        {
            // Wait for main thread
            while (!LagLimiter.ShouldContinue(60))
                yield return null;
            
            // Wait for readiness
            while (!IsReady())
                yield return null;

            // Render sprite
            _spriteRenderer.sprite = TryGetFrameSprite(_frameIndex);

            // Wait for next frame
            yield return new WaitForSeconds(TryGetFrameDelay(_frameIndex));

            // Update frame index
            _frameIndex = reverse ? _frameIndex - 1 : _frameIndex + 1;

            // Keep frame in bounds
            var isOutOfBounds = _frameIndex < 0 || _frameIndex >= _frameCount;
            if (_frameCount > 0) // <-- Prevent division by zero
                _frameIndex = (_frameIndex + _frameCount) % _frameCount;

            // Stop if out of bounds
            if (isOutOfBounds && !repeat)
                Stop(!reverse);
        }
    }

    /// <summary>
    /// Tries to get the sprite for a given frame, returning null on error
    /// </summary>
    /// <param name="frame">Index of the frame, starting at 0</param>
    /// <returns>The sprite for the given frame, or null on error</returns>
    private Sprite? TryGetFrameSprite(int frame)
    {
        try
        {
            return GetFrameSprite(frame);
        }
        catch
        {
            LILogger.Info($"Problem loading {name}'s animation frame (frame {frame})");
            return null;
        }
    }
    
    /// <summary>
    /// Tries to get the delay for a given frame, returning 0.0f on error
    /// </summary>
    /// <param name="frame">Index of the frame, starting at 0</param>
    /// <returns>The delay for the given frame in seconds, or 0.0f on error</returns>
    private float TryGetFrameDelay(int frame)
    {
        try
        {
            return GetFrameDelay(frame);
        }
        catch
        {
            LILogger.Info($"Problem loading {name}'s animation delay (frame {frame})");
            return 0.1f;
        }
    }

    /// <summary>
    /// Checks if an animation is available and ready to play
    /// </summary>
    /// <returns>TRUE if the animation is ready</returns>
    /// <exception cref="NotImplementedException">Thrown if not implemented by subclass</exception>
    protected virtual bool IsReady()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Fires when the animator is cloned
    /// </summary>
    /// <param name="originalAnim">The original animator</param>
    protected virtual void OnClone(LIAnimatorBase originalAnim)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the sprite for a given frame
    /// </summary>
    /// <param name="frame">Index of the frame, starting at 0</param>
    /// <returns>The sprite for the given frame</returns>
    protected virtual Sprite GetFrameSprite(int frame)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the delay for a given frame
    /// </summary>
    /// <param name="frame">Index of the frame, starting at 0</param>
    /// <returns>The delay for the given frame in seconds</returns>
    protected virtual float GetFrameDelay(int frame)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the total number of frames in the animation
    /// </summary>
    /// <returns>The total number of frames</returns>
    protected virtual int GetFrameCount()
    {
        throw new NotImplementedException();
    }
}