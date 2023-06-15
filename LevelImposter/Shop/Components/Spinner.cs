using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Shop
{
    /// <summary>
    /// Just a simple spinning object
    /// </summary>
    public class Spinner : MonoBehaviour
    {
        public Spinner(IntPtr intPtr) : base(intPtr)
        {
        }

        private float _speed = -90f;

        public void Update()
        {
            transform.Rotate(0, 0, _speed * Time.deltaTime);
        }
    }
}