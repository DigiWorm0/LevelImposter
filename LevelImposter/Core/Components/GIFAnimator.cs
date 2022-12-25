using System;
using System.IO;
using System.Collections;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace LevelImposter.Core
{
    /// <summary>
    /// Component to animate GIF data in-game
    /// </summary>
    public class GIFAnimator : MonoBehaviour
    {
        public bool IsAnimating = false;

        private float[] _delays;
        private Sprite[] _frames;
        private SpriteRenderer _spriteRenderer;
        private Coroutine _animationCoroutine = null;

        public GIFAnimator(IntPtr intPtr) : base(intPtr)
        {
        }

        /// <summary>
        /// Initializes the component with GIF data
        /// </summary>
        /// <param name="base64">GIF data in base-64 format</param>
        public void Init(string base64)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            string sub64 = base64.Substring(base64.IndexOf(",") + 1);
            byte[] data = Convert.FromBase64String(sub64);
            MemoryStream stream = new MemoryStream(data);
            GIFLoader gifLoader = new GIFLoader();
            GIFImage image = gifLoader.Load(stream);

            _frames = image.GetFrames();
            _delays = image.GetDelays();

            foreach (Sprite frame in _frames)
                LIShipStatus.Instance.AddMapTexture(frame.texture);
        }

        /// <summary>
        /// Plays the GIF animation
        /// </summary>
        /// <param name="repeat">True if the GIF should repeat. False otherwise</param>
        public void Play(bool repeat = false, bool reverse = false)
        {
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
            IsAnimating = false;
            _spriteRenderer.sprite = _frames[reversed ? _frames.Length - 1 : 0];
        }

        private IEnumerator CoAnimate(bool repeat, bool reverse)
        {
            IsAnimating = true;
            int t = 0;
            while (IsAnimating)
            {
                int frame = reverse ? _frames.Length - t - 1 : t;
                _spriteRenderer.sprite = _frames[frame];
                yield return new WaitForSeconds(_delays[frame]);
                t = (t + 1) % _frames.Length;
                if (t == 0 && !repeat)
                    Stop(!reverse);
            }
        }
    }
}
