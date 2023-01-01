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

        /// <summary>
        /// Sets the rate of rotation for the spinner
        /// </summary>
        /// <param name="speed">Speed measured in deg/sec</param>
        public void SetSpeed(float speed)
        {
            _speed = speed;
        }

        public void Update()
        {
            transform.Rotate(0, 0, _speed * Time.deltaTime);
        }
    }
}