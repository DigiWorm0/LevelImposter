using System;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Core
{
    /// <summary>
    /// Object that flies across screen and respawns when it hits the edge
    /// </summary>
    public class LIStar : MonoBehaviour
    {
        public LIStar(IntPtr intPtr) : base(intPtr)
        {
        }

        private float _height = 10;
        private float _length = 10;
        private float _minSpeed = 2;
        private float _maxSpeed = 2;
        private float _currentSpeed = 0;

        /// <summary>
        /// Initializes a star from util-starfield
        /// </summary>
        /// <param name="elem">LIElement to extract props from</param>
        [HideFromIl2Cpp]
        public void Init(LIElement elem)
        {
            _height = elem.properties.starfieldHeight ?? _height;
            _length = elem.properties.starfieldLength ?? _length;
            _minSpeed = elem.properties.starfieldMinSpeed ?? _minSpeed;
            _maxSpeed = elem.properties.starfieldMaxSpeed ?? _maxSpeed;
            elem = null;
        }

        /// <summary>
        /// Respawns the Star in the Star Field
        /// </summary>
        /// <param name="isInitial">TRUE will also randomize the X position</param>
        private void Respawn(bool isInitial)
        {
            _currentSpeed = UnityEngine.Random.Range(_minSpeed, _maxSpeed);
            transform.localPosition = new Vector3(
                isInitial ? UnityEngine.Random.Range(-_length, 0) : 0,
                UnityEngine.Random.Range(-_height / 2, _height / 2),
                0
            );
        }

        public void Start()
        {
            Respawn(true);
        }
        public void Update()
        {
            transform.localPosition -= new Vector3(
                _currentSpeed * Time.deltaTime,
                0,
                0
            );
            if (transform.localPosition.x < -_length)
                Respawn(false);
        }
    }
}
