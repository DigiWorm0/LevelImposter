using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Shop
{
    public class Spinner : MonoBehaviour
    {
        public Spinner(IntPtr intPtr) : base(intPtr)
        {
        }

        public float Speed = -90f;

        private void Update()
        {
            transform.Rotate(0, 0, Speed * Time.deltaTime);
        }
    }
}