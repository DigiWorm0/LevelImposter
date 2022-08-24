using System;
using System.IO;
using System.Collections;
using UnityEngine;
using BepInEx.IL2CPP.Utils.Collections;

namespace LevelImposter.Core
{
    public class GIFAnimator : MonoBehaviour
    {
        public bool isAnimating = false;
        public Sprite[] frames;
        public float[] delays;

        private SpriteRenderer spriteRenderer;

        public void Init(string base64)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            string sub64 = base64.Substring(base64.IndexOf(",") + 1);
            byte[] data = Convert.FromBase64String(sub64);
            MemoryStream stream = new MemoryStream(data);
            GIFLoader gifLoader = new GIFLoader();
            GIFImage image = gifLoader.Load(stream);

            frames = image.GetFrames();
            delays = image.GetDelays();

        }

        public void Play(bool repeat)
        {
            if (isAnimating)
                StopAllCoroutines();
            StartCoroutine(CoAnimate(repeat).WrapToIl2Cpp());
        }

        public void Stop()
        {
            StopAllCoroutines();
            isAnimating = false;
            spriteRenderer.sprite = frames[0];
        }

        public IEnumerator CoAnimate(bool repeat)
        {
            isAnimating = true;
            int f = 0;
            while (isAnimating)
            {
                spriteRenderer.sprite = frames[f];
                yield return new WaitForSeconds(delays[f]);
                f = (f + 1) % frames.Length;
                if (f == 0 && !repeat)
                    Stop();
            }
        }
    }
}
