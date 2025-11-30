using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.AssetLoader;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Component to animate LI's Sprite Animations in-game
/// </summary>
public class SpriteAnimator(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private static readonly Dictionary<long, SpriteAnimator> _allAnimators = new();
    private static long _nextAnimatorID = 1;

    private static readonly List<string> AUTOPLAY_BLACKLIST =
    [
        "util-vent1",
        "util-vent2",
        "sab-doorv",
        "sab-doorh",
        "util-cam"
    ];

    private Coroutine? _animationCoroutine;

    private int _frame;
    private bool _loop;
    private LISpriteAnimation? _animationData;
    private SpriteRenderer? _spriteRenderer;

    public Il2CppValueField<long> AnimatorID; // Unique ID maintained on instantiation

    public bool IsAnimating { get; private set; }

    private long _animatorID => AnimatorID.Get();

    public void Awake()
    {
        // Check if object was cloned
        if (_animatorID != 0)
        {
            var objectExists = _allAnimators.TryGetValue(_animatorID, out var originalObject);
            if (objectExists && originalObject != null)
                OnClone(originalObject);
        }

        // Update Object ID
        AnimatorID.Set(_nextAnimatorID++);
        _allAnimators.Add(_animatorID, this);
    }

    public void OnDestroy()
    {
        _allAnimators.Remove(_animatorID);

        _animationData = null;
        _spriteRenderer = null;
        _animationCoroutine = null;
    }

    /// <summary>
    ///     Initializes the component with GIF data
    /// </summary>
    /// <param name="element">Element that is initialized</param>
    /// <param name="animationData">Data to animate</param>
    [HideFromIl2Cpp]
    public void Init(LIElement element, LISpriteAnimation? animationData)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animationData = animationData;
        _loop = element.properties.loopGIF ?? true; 
        
        Play();
    }

    /// <summary>
    ///     Plays the GIF animation with default options
    /// </summary>
    public void Play()
    {
        Play(_loop, false);
    }

    /// <summary>
    ///     Plays the GIF animation with custom options
    /// </summary>
    /// <param name="repeat">True iff the GIF should loop</param>
    /// <param name="reverse">True iff the GIF should play in reverse</param>
    public void Play(bool repeat, bool reverse)
    {
        if (_animationData == null)
            LILogger.Warn($"{name} does not have any data");
        if (_spriteRenderer == null)
            LILogger.Warn($"{name} does not have a spriteRenderer");
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(CoAnimate(repeat, reverse).WrapToIl2Cpp());
    }

    /// <summary>
    ///     Stops the GIF animation
    /// </summary>
    public void Stop(bool reversed = false)
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        IsAnimating = false;

        if (_spriteRenderer == null || _animationData == null)
            return;

        _frame = reversed ? _animationData.frames.Length - 1 : 0;
        _spriteRenderer.sprite = GetFrameSprite(_frame);
        _spriteRenderer.enabled = true;
    }
    
    /// <summary>
    /// Gets a frame sprite asynchronously
    /// </summary>
    /// <param name="frameIndex">Index of the frame</param>
    /// <param name="callback">Callback when the frame is fetched</param>
    private Sprite GetFrameSprite(int frameIndex)
    {
        if (_animationData == null)
            throw new Exception("Animation data is null");

        // Get Frame Data
        var frame = _animationData.frames[frameIndex % _animationData.frames.Length];
        
        // Get MapAsset
        var mapAssetDB = MapLoader.CurrentMap?.mapAssetDB;
        var mapAsset = mapAssetDB?.Get(frame.spriteID);
        if (mapAsset == null)
            throw new Exception("Animation sprite not found in MapAssetDB");

        // Load Sprite
        // TODO: Do this asynchronously
        return SpriteLoader.LoadSync(
            frame.spriteID.ToString(),
            mapAsset
        );
    }

    /// <summary>
    ///     Coroutine to run GIF animation
    /// </summary>
    /// <param name="repeat">TRUE if animation should loop</param>
    /// <param name="reverse">TRUE if animation should run in reverse</param>
    /// <returns>IEnumerator for Unity Coroutine</returns>
    [HideFromIl2Cpp]
    private IEnumerator CoAnimate(bool repeat, bool reverse)
    {
        if (_animationData == null || _spriteRenderer == null)
            yield break;
        // Flag Start
        IsAnimating = true;
        _spriteRenderer.enabled = true;

        // Reset frame
        if (reverse && _frame == 0)
            _frame = _animationData.frames.Length - 1;
        else if (!reverse && _frame == _animationData.frames.Length - 1)
            _frame = 0;

        // Loop
        while (IsAnimating)
        {
            // Wait for main thread
            while (!LagLimiter.ShouldContinue(60))
                yield return null;

            // Render sprite
            _spriteRenderer.sprite = GetFrameSprite(_frame);

            // Wait for next frame
            yield return new WaitForSeconds(_animationData.frames[_frame].delay / 1000f);

            // Update frame index
            _frame = reverse ? _frame - 1 : _frame + 1;

            // Keep frame in bounds
            var isOutOfBounds = _frame < 0 || _frame >= _animationData.frames.Length;
            _frame = (_frame + _animationData.frames.Length) % _animationData.frames.Length;

            // Stop if out of bounds
            if (isOutOfBounds && !repeat)
                Stop(!reverse);
        }
    }

    /// <summary>
    ///     Fires when the animator is cloned
    /// </summary>
    /// <param name="original"></param>
    private void OnClone(SpriteAnimator originalAnim)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animationData = originalAnim._animationData;
        _loop = originalAnim._loop;
    }
}