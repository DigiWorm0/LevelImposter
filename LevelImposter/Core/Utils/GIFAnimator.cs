using System;
using System.IO;
using System.Collections;
using UnityEngine;
using BepInEx.IL2CPP.Utils.Collections;

namespace LevelImposter.Core
{
    public class GIFAnimator : MonoBehaviour
    {
        public bool IsAnimating = false;

        private float[] _delays;
        private Sprite[] _frames;
        private SpriteRenderer _spriteRenderer;

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

        }

        public void Play(bool repeat)
        {
            if (IsAnimating)
                StopAllCoroutines();
            StartCoroutine(CoAnimate(repeat).WrapToIl2Cpp());
        }

        public void Stop()
        {
            StopAllCoroutines();
            IsAnimating = false;
            _spriteRenderer.sprite = _frames[0];
        }

        public IEnumerator CoAnimate(bool repeat)
        {
            IsAnimating = true;
            int f = 0;
            while (IsAnimating)
            {
                _spriteRenderer.sprite = _frames[f];
                yield return new WaitForSeconds(_delays[f]);
                f = (f + 1) % _frames.Length;
                if (f == 0 && !repeat)
                    Stop();
            }
        }
    }
}
